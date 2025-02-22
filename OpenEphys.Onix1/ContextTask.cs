using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Encapsulates a single ONI context and orchestrates interaction with ONI-compliant hardware.
    /// </summary>
    /// <remarks>
    /// The <see href="https://open-ephys.github.io/ONI/">Open Neuro Interface (ONI)</see>
    /// specification describe a general purpose acquisition system architecture and programming
    /// interface for communication with a host PC. One requirement of ONI is that a host application must
    /// hold a "context" that contains handles for hardware communication, data acquisition parameters, etc.
    /// for a particular ONI Controller, such as the ONIX PCIe card. <see cref="ContextTask"/> fulfills
    /// this role for this library. Additionally, once data acquisition is started by the <see
    /// cref="StartAcquisition"/> operator, <see cref="ContextTask"/> performs the following:
    /// <list type="bullet">
    /// <item><description>It automatically reads and distributes data from hardware using a dedicated acquisition
    /// thread.</description></item>
    /// <item><description>It allows data to be written to devices that accept them.</description></item>
    /// <item><description>It allows reading from and writing to device registers to control their operation (e.g. <see
    /// cref="ConfigureBno055.Enable"/> or <see
    /// cref="ConfigureRhd2164.AnalogHighCutoff"/>).</description></item>
    /// </list>
    /// Additionally, this operator exposes important information about the underlying ONI hardware such as
    /// the device table, clock rates, and block read and write sizes. <strong>In summary, <see
    /// cref="ContextTask"/> forms a complete interface for all hardware interaction within the library: all
    /// physical interaction with the ONIX system ultimately passes through this class.</strong>
    /// </remarks>
    public class ContextTask : IDisposable
    {
        readonly oni.Context ctx;

        /// <summary>
        /// Maximum amount of frames the reading queue will hold. If the queue fills or the read thread is not
        /// performant enough to fill it faster than data is produced, frame reading will throttle, filling
        /// host memory instead of user space memory.
        /// </summary>
        const int MaxQueuedFrames = 2_000_000;

        /// <summary>
        /// Timeout in ms for queue reads. This should not be critical as the read operation will cancel if
        /// the task is stopped
        /// </summary>
        const int QueueTimeoutMilliseconds = 200;

        internal const string DefaultDriver = "riffa";
        internal const int DefaultIndex = 0;

        // NB: Decouple OnNext() from hardware reads
        bool disposed;
        Task readFrames;
        Task distributeFrames;
        Task acquisition = Task.CompletedTask;
        CancellationTokenSource collectFramesCancellation;
        event Func<ContextTask, IDisposable> ConfigureHostEvent;
        event Func<ContextTask, IDisposable> ConfigureLinkEvent;
        event Func<ContextTask, IDisposable> ConfigureDeviceEvent;

        // FrameReceived observable sequence
        readonly Subject<oni.Frame> frameReceived = new();
        readonly IConnectableObservable<IGroupedObservable<uint, oni.Frame>> groupedFrames;

        // TODO: These work for RIFFA implementation, but potentially not others!!
        readonly object readLock = new();
        readonly object writeLock = new();
        readonly object regLock = new();
        readonly object disposeLock = new();

        readonly string contextDriver = DefaultDriver;
        readonly int contextIndex = DefaultIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTask"/> class.
        /// </summary>
        /// <param name="driver"> A string specifying the device driver used to control hardware. </param>
        /// <param name="index">The index of the host interconnect between the ONI controller and host
        /// computer. For instance, 0 could correspond to a particular PCIe slot or USB port as enumerated by
        /// the operating system and translated by an <see href="https://open-ephys.github.io/ONI/">ONI</see>
        /// Device Driver Translator. A value of -1 will attempt to open the default hardware index and is
        /// useful if there is only a single ONI controller managed by the specified <paramref name="driver"/>
        /// in the host computer.</param>
        internal ContextTask(string driver, int index)
        {
            groupedFrames = frameReceived.GroupBy(frame => frame.DeviceAddress).Replay();
            groupedFrames.Connect();
            contextDriver = driver;
            contextIndex = index;
            ctx = new oni.Context(contextDriver, contextIndex);
            Initialize();
        }

        private void Initialize()
        {
            SystemClockHz = ctx.SystemClockHz;
            AcquisitionClockHz = ctx.AcquisitionClockHz;
            MaxReadFrameSize = ctx.MaxReadFrameSize;
            MaxWriteFrameSize = ctx.MaxWriteFrameSize;
            DeviceTable = ctx.DeviceTable;
        }

        internal void Reset()
        {
            lock (disposeLock)
                lock (regLock)
                {
                    AssertConfigurationContext();
                    lock (readLock)
                        lock (writeLock)
                        {
                            ctx.Refresh();
                            Initialize();
                        }
                }
        }

        /// <summary>
        /// Gets the system clock rate in Hz.
        /// </summary>
        /// <remarks>
        /// This property describes the frequency of the clock governing the ONI controller. The value of this
        /// property is determined during hardware initialization.
        /// </remarks>
        public uint SystemClockHz { get; private set; }

        /// <summary>
        /// Gets the acquisition clock rate in Hz.
        /// </summary>
        /// <remarks>
        /// This property describes the frequency of the <see
        /// href="https://open-ephys.github.io/ONI/">ONI</see> Controller's Acquisition Clock, which is used
        /// to generate the <see cref="DataFrame.Clock">Clock</see> counter value included in all data frames
        /// produced by Data IO operators in this library (e.g. <see cref="NeuropixelsV1eData"/> or <see
        /// cref="Bno055Data"/>). The value of this property is determined during hardware initialization.
        /// </remarks>
        public uint AcquisitionClockHz { get; private set; }

        /// <summary>
        /// Gets the size of the largest data frame produced by any device with the acquisition system in bytes.
        /// </summary>
        /// <remarks>
        /// This number describes the the size, in bytes, of the largest <see
        /// href="https://open-ephys.github.io/ONI/">ONI</see> Data Frame produced by any device within the
        /// current device table that generates data. Therefore, it also defines the lower bound for the value
        /// of <see cref="BlockReadSize"/>. The value of this property is determined during hardware
        /// initialization.
        /// </remarks>
        public uint MaxReadFrameSize { get; private set; }

        /// <summary>
        /// Gets the size of the largest data frame consumed by any device with the acquisition system in bytes.
        /// </summary>
        /// <remarks>
        /// This number describes the the size, in bytes, of the largest <see
        /// href="https://open-ephys.github.io/ONI/">ONI</see> Data Frame consumed by any device within the
        /// current device table that accepts data. Therefore, it also defines the lower bound for the value
        /// of <see cref="BlockWriteSize"/>. The value of this property is determined during hardware
        /// initialization.
        /// </remarks>
        public uint MaxWriteFrameSize { get; private set; }

        /// <summary>
        /// Gets the device table containing the device hierarchy of the acquisition system.
        /// </summary>
        /// <remarks>
        /// This dictionary provides access to the <see href="https://open-ephys.github.io/ONI/">ONI</see>
        /// Device Table, which maps a set of fully-qualified Device Addresses to a corresponding set of
        /// Device Descriptors. The value of this property is determined during hardware initialization.
        /// </remarks>
        public Dictionary<uint, oni.Device> DeviceTable { get; private set; }

        internal IObservable<IGroupedObservable<uint, oni.Frame>> GroupedFrames => groupedFrames;

        /// <summary>
        /// Gets the sequence of <see href="https://open-ephys.github.io/ONI/">ONI</see> Data Frames produced
        /// by a particular device.
        /// </summary>
        /// <param name="deviceAddress">The fully-qualified <see
        /// href="https://open-ephys.github.io/ONI/">ONI</see> Device Address of the Device that will produce
        /// the Data Frame sequence.</param>
        /// <returns>The frame sequence produced by the device at address <paramref
        /// name="deviceAddress"/>.</returns>
        public IObservable<oni.Frame> GetDeviceFrames(uint deviceAddress)
        {
            return groupedFrames.Where(deviceFrames => deviceFrames.Key == deviceAddress).Merge();
        }

        void AssertConfigurationContext()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (!acquisition.IsCompleted)
            {
                throw new InvalidOperationException("Configuration cannot be changed while acquisition context is running.");
            }
        }

        // NB: This is where actions that reconfigure the hub state, or otherwise
        // change the device table should be executed
        internal void ConfigureHost(Func<ContextTask, IDisposable> configure)
        {
            lock (regLock)
            {
                AssertConfigurationContext();
                ConfigureHostEvent += configure;
            }
        }

        // NB: This is where actions that calibrate port voltage or otherwise
        // check link lock state should be executed
        internal void ConfigureLink(Func<ContextTask, IDisposable> configure)
        {
            lock (regLock)
            {
                AssertConfigurationContext();
                ConfigureLinkEvent += configure;
            }
        }

        // NB: Actions queued using this method should assume that the device table
        // is finalized and cannot be changed
        internal void ConfigureDevice(Func<ContextTask, IDisposable> configure)
        {
            lock (regLock)
            {
                AssertConfigurationContext();
                ConfigureDeviceEvent += configure;
            }
        }

        private IDisposable ConfigureContext()
        {
            var hostAction = Interlocked.Exchange(ref ConfigureHostEvent, null);
            var linkAction = Interlocked.Exchange(ref ConfigureLinkEvent, null);
            var deviceAction = Interlocked.Exchange(ref ConfigureDeviceEvent, null);
            var disposable = new StackDisposable();
            ConfigureResources(disposable, hostAction);
            ConfigureResources(disposable, linkAction);
            ConfigureResources(disposable, deviceAction);
            return disposable;
        }

        void ConfigureResources(StackDisposable disposable, Func<ContextTask, IDisposable> action)
        {
            if (action != null)
            {
                var invocationList = action.GetInvocationList();
                try
                {
                    foreach (var selector in invocationList.Cast<Func<ContextTask, IDisposable>>())
                    {
                        disposable.Push(selector(this));
                    }
                }
                catch
                {
                    disposable.Dispose();
                    throw;
                }
                finally { Reset(); }
            }
        }

        internal Task StartAsync(int blockReadSize, int blockWriteSize, CancellationToken cancellationToken = default)
        {
            lock (disposeLock)
                lock (regLock)
                {
                    if (disposed)
                    {
                        throw new ObjectDisposedException(GetType().FullName);
                    }

                    if (!acquisition.IsCompleted)
                    {
                        throw new InvalidOperationException("Acquisition already running in the current context.");
                    }

                    // NB: Configure context before starting acquisition or the the settings (e.g. Block read
                    // and write sizes) will not be respected
                    var contextConfiguration = ConfigureContext();

                    try
                    {
                        // set block read and write size
                        ctx.BlockReadSize = blockReadSize;
                        ctx.BlockWriteSize = blockWriteSize;

                        // TODO: Stuff related to sync mode is 100% ONIX, not ONI. Therefore, in the long term,
                        // another place to do this separation might be needed
                        int address = ctx.HardwareAddress;
                        int mode = (address & 0x00FF0000) >> 16;
                        if (mode == 0) // Standalone mode
                        {
                            ctx.Start(true);
                        }
                        else // If synchronized mode, reset counter independently
                        {
                            ctx.ResetFrameClock();
                            ctx.Start(false);
                        }

                    }
                    catch (oni.ONIException ex) when (ex.Number == -20)
                    {
                        lock (regLock)
                        {
                            ctx.Stop();
                            contextConfiguration.Dispose();
                        }
                        throw new InvalidOperationException($"The requested read size of {blockReadSize} bytes is too small for the current " +
                            $"hardware configuration, which requires at least {ctx.MaxReadFrameSize} bytes.", ex);
                    }
                    catch (oni.ONIException ex) when (ex.Number == -24)
                    {
                        lock (regLock)
                        {
                            ctx.Stop();
                            contextConfiguration.Dispose();
                        }
                        throw new InvalidOperationException($"The requested write size of {blockWriteSize} bytes is too small for the current " +
                            $"hardware configuration, which requires at least {ctx.MaxWriteFrameSize} bytes.", ex);
                    }
                    catch
                    {
                        lock (regLock)
                        {
                            ctx.Stop();
                            contextConfiguration.Dispose();
                        }
                        throw;
                    }

                    // TODO: If during the creation of of collectFramesCancellation, collectFramesToken, frameQueue, readFrames, or distributeFrames
                    // an exception is thrown, contextConfiguration will not be disposed. The process will need to be restarted to get out of deadlock
                    collectFramesCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    var collectFramesToken = collectFramesCancellation.Token;
                    var frameQueue = new BlockingCollection<oni.Frame>(MaxQueuedFrames);

                    readFrames = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (!collectFramesToken.IsCancellationRequested)
                            {
                                // NB: This is a blocking call and there is no safe way to terminate it
                                // other than ending the process. For this reason, it is the job of the 
                                // hardware to provide enough data (e.g. through a HeartbeatDevice") for
                                // this call to return.
                                oni.Frame frame;
                                try { frame = ReadFrame(); }
                                catch (Exception)
                                {
                                    collectFramesCancellation.Cancel();
                                    throw;
                                }
                                frameQueue.Add(frame, collectFramesToken);

                            }
                        }
                        catch (OperationCanceledException)
                        {
#if DEBUG
                            // NB: If FrameQueue.Add has not been called, frame has ref count 0 when it exits
                            // while loop context and will be disposed.
                            Console.WriteLine("Frame collection task has been cancelled by " + this.GetType());
#endif
                        };
                    },
                    collectFramesToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                    distributeFrames = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (!collectFramesToken.IsCancellationRequested)
                            {
                                if (frameQueue.TryTake(out oni.Frame frame, QueueTimeoutMilliseconds, collectFramesToken))
                                {
                                    frameReceived.OnNext(frame);
                                    frame.Dispose();
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
#if DEBUG
                            // NB: If the thread stops no frame has been collected
                            Console.WriteLine("Frame distribution task has been cancelled by " + this.GetType());
#endif
                        }
                    },
                    collectFramesToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                    return acquisition = Task.WhenAll(distributeFrames, readFrames).ContinueWith(task =>
                    {
                        if (readFrames.IsFaulted && readFrames.Exception is AggregateException ex)
                        {
                            var error = ex.InnerExceptions.Count == 1 ? ex.InnerExceptions[0] : ex;
                            frameReceived.OnError(error);
                        }

                        lock (regLock)
                        {
                            collectFramesCancellation?.Dispose();
                            collectFramesCancellation = null;

                            // Clear queue and free memory
                            while (frameQueue?.Count > 0)
                            {
                                var frame = frameQueue.Take();
                                frame.Dispose();
                            }
                            frameQueue?.Dispose();
                            frameQueue = null;
                            ctx.Stop();

                            contextConfiguration.Dispose();
                            acquisition = Task.CompletedTask;
                        }
                    });
                }
        }

        #region oni.Context Properties

        /// <summary>
        /// Gets the data acquisition run state.
        /// </summary>
        /// <remarks>
        /// A value of true indicates that data is being acquired by the host computer from the host
        /// controller. False indicates that the host computer is not collecting data from the controller and
        /// that the controller memory remains cleared.
        /// </remarks>
        internal bool Running => ctx.Running;

        internal int HardwareAddress
        {
            get => ctx.HardwareAddress;
            set => ctx.HardwareAddress = value;
        }

        /// <summary>
        /// Gets the number of bytes read per cycle of the <see cref="ContextTask"/>'s acquisition thread.
        /// </summary>
        /// <inheritdoc cref = "StartAcquisition.ReadSize"/>
        public int BlockReadSize => ctx.BlockReadSize;

        /// <summary>
        /// Gets the number of bytes that are pre-allocated for writing data to hardware.
        /// </summary>
        /// <inheritdoc cref = "StartAcquisition.WriteSize"/>
        public int BlockWriteSize => ctx.BlockWriteSize;

        // Port A and Port B each have a bit in PORTFUNC
        internal PassthroughState HubState
        {
            get => (PassthroughState)ctx.GetCustomOption((int)oni.ONIXOption.PORTFUNC);
            set => ctx.SetCustomOption((int)oni.ONIXOption.PORTFUNC, (int)value);
        }

        // NB: This is for actions that require synchronized register access and might
        // be called asynchronously with context dispose
        internal void EnsureContext(Action action)
        {
            lock (disposeLock)
            {
                if (!disposed)
                {
                    action();
                }
            }
        }

        internal uint ReadRegister(uint deviceAddress, uint registerAddress)
        {
            lock (regLock)
            {
                return ctx.ReadRegister(deviceAddress, registerAddress);
            }
        }

        internal void WriteRegister(uint deviceAddress, uint registerAddress, uint value)
        {
            lock (regLock)
            {
                ctx.WriteRegister(deviceAddress, registerAddress, value);
            }
        }

        private oni.Frame ReadFrame()
        {
            lock (readLock)
            {
                return ctx.ReadFrame();
            }
        }

        internal void Write<T>(uint deviceAddress, T data) where T : unmanaged
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data);
            }
        }

        internal void Write<T>(uint deviceAddress, T[] data) where T : unmanaged
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data);
            }
        }

        internal void Write(uint deviceAddress, IntPtr data, int dataSize)
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data, dataSize);
            }
        }

        internal oni.Hub GetHub(uint deviceAddress) => ctx.GetHub(deviceAddress);

        internal uint GetPassthroughDeviceAddress(uint deviceAddress)
        {
            var hubAddress = (deviceAddress & 0xFF00u) >> 8;
            if (hubAddress == 0)
            {
                throw new ArgumentException(
                    "Device addresses on hub zero cannot be used to create passthrough devices.",
                    nameof(deviceAddress));
            }

            return hubAddress + 7;
        }

        #endregion

        private void DisposeContext()
        {
            lock (disposeLock)
                lock (regLock)
                    lock (readLock)
                        lock (writeLock)
                        {
                            ctx.Dispose();
                        }
        }

        /// <summary>
        /// Dispose the <see cref="ContextTask"/> and free all resources.
        /// </summary>
        public void Dispose()
        {
            lock (disposeLock)
                lock (regLock)
                {
                    disposed = true;
                    acquisition.ContinueWith(_ => DisposeContext());
                    collectFramesCancellation?.Cancel();
                }

            GC.SuppressFinalize(this);
        }
    }
}
