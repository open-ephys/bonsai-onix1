using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Encapsulates an <see cref="oni.Context"/> and orchestrates interaction with ONI hardware.
    /// </summary>
    /// <remarks>
    /// This class forms the basis for ONI hardware interaction within the library. It manages an <see cref="oni.Context"/>. It
    /// reads and distributes <see cref="oni.Frame"/>s using a dedicated acquisition thread. It allows <see cref="oni.Frame"/>s to
    /// be written to devices that accept them. Finally, it exposes information about the underlying ONI hardware such as the device
    /// table, clock rates, and block read and write sizes.
    /// </remarks>
    public class ContextTask : IDisposable
    {
        oni.Context ctx;

        /// <summary>
        /// Maximum amount of frames the reading queue will hold. If the queue fills or the read
        /// thread is not performant enough to fill it faster than data is produced, frame reading
        /// will throttle, filling host memory instead of user space memory.
        /// </summary>
        const int MaxQueuedFrames = 2_000_000;

        /// <summary>
        /// Timeout in ms for queue reads. This should not be critical as the read operation will
        /// cancel if the task is stopped
        /// </summary>
        const int QueueTimeoutMilliseconds = 200;

        /// <summary>
        /// In this package most operators are tied in to the RIFFA PCIe backend used by the FMC host.
        /// </summary>
        internal const string DefaultDriver = "riffa";
        internal const int DefaultIndex = 0;

        // NB: Decouple OnNext() form hardware reads
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
        /// <param name="index">The index of the host interconnect between the ONI controller and host computer. For instance, 0 could
        /// correspond to a particular PCIe slot or USB port as enumerated by the operating system and translated by an
        /// <see href="https://open-ephys.github.io/ONI/api/liboni/driver-translators/index.html#drivers">ONI device driver translator</see>. 
        /// A value of -1 will attempt to open the default hardware index and is useful if there is only a single ONI controller
        /// managed by the specified <paramref name="driver"/> in the host computer.</param>
        internal ContextTask(string driver, int index)
        {
            groupedFrames = frameReceived.GroupBy(frame => frame.DeviceAddress).Replay();
            groupedFrames.Connect();
            contextDriver = driver;
            contextIndex = index;
            Initialize();
        }

        private void Initialize()
        {
            ctx = new oni.Context(contextDriver, contextIndex);
            SystemClockHz = ctx.SystemClockHz;
            AcquisitionClockHz = ctx.AcquisitionClockHz;
            MaxReadFrameSize = ctx.MaxReadFrameSize;
            MaxWriteFrameSize = ctx.MaxWriteFrameSize;
            DeviceTable = ctx.DeviceTable;
        }

        private void Reset()
        {
            lock (disposeLock)
                lock (regLock)
                {
                    AssertConfigurationContext();
                    lock (readLock)
                        lock (writeLock)
                        {
                            ctx?.Dispose();
                            Initialize();
                        }
                }
        }

        /// <summary>
        /// Gets the system clock rate in Hz.
        /// </summary>
        /// <remarks>
        /// This describes the frequency of the clock governing the controller hardware.
        /// </remarks>
        public uint SystemClockHz { get; private set; }

        /// <summary>
        /// Gets the acquisition clock rate in Hz.
        /// </summary>
        /// <remarks>
        /// This describes the frequency of the clock used to drive the acquisition counter which is used to provide the clock
        /// counter values in <see cref="oni.Frame.Clock"/> and its derivative types (e.g. <see cref="DataFrame.Clock"/>,
        /// <see cref="BufferedDataFrame.Clock"/>, etc.)
        /// </remarks>
        public uint AcquisitionClockHz { get; private set; }

        /// <summary>
        /// Gets the maximal size of a frame produced by a call to <see cref="oni.Context.ReadFrame"/> in bytes.
        /// </summary>
        /// <remarks>
        /// This number is the maximum sized frame that can be produced across every device within the device table
        /// that generates data.
        /// </remarks>
        public uint MaxReadFrameSize { get; private set; }

        /// <summary>
        /// Gets the maximal size consumed by a call to <see cref="oni.Context.Write"/> in bytes. 
        /// </summary>
        /// <remarks>
        /// This number is the maximum sized frame that can be consumed across every device within the device table
        /// that accepts write data.
        /// </remarks>
        public uint MaxWriteFrameSize { get; private set; }

        /// <summary>
        /// Gets the device table containing the full device hierarchy governed by the internal <see cref="oni.Context"/>.
        /// </summary>
        /// <remarks>
        /// This dictionary maps a fully-qualified <see cref="oni.Device.Address"/> to an <see cref="oni.Device"/> instance.
        /// </remarks>
        public Dictionary<uint, oni.Device> DeviceTable { get; private set; }

        internal IObservable<IGroupedObservable<uint, oni.Frame>> GroupedFrames => groupedFrames;

        /// <summary>
        /// Gets the sequence of <see cref="oni.Frame"/>s produced by a particular device.
        /// </summary>
        /// <param name="deviceAddress">The fully qualified <see cref="oni.Device.Address"/> that will produce the frame sequence.</param>
        /// <returns>The frame sequence produced by the device at address <paramref name="deviceAddress"/>.</returns>
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
                        throw new InvalidOperationException("Acquisition already running in the current context.");

                    // NB: Configure context before starting acquisition
                    var contextConfiguration = ConfigureContext();
                    ctx.BlockReadSize = blockReadSize;
                    ctx.BlockWriteSize = blockWriteSize;

                    // NB: Stuff related to sync mode is 100% ONIX, not ONI, so long term another place
                    // to do this separation might be needed
                    int addr = ctx.HardwareAddress;
                    int mode = (addr & 0x00FF0000) >> 16;
                    if (mode == 0) // Standalone mode
                    {
                        ctx.Start(true);
                    }
                    else // If synchronized mode, reset counter independently
                    {
                        ctx.ResetFrameClock();
                        ctx.Start(false);
                    }

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
        /// Gets the data acquisition state. 
        /// </summary>
        /// <remarks>
        /// A value of true indicates that data is being acquired by the host computer from the host controller.
        /// False indicates that the host computer is not collecting data from the controller and that the controller
        /// memory remains cleared.
        /// </remarks>
        internal bool Running => ctx.Running;

        public int HardwareAddress
        {
            get => ctx.HardwareAddress;
            set => ctx.HardwareAddress = value;
        }

        /// <summary>
        /// Gets the number of bytes read by the device driver access to the read channel.
        /// </summary>
        /// <remarks>
        /// This option allows control over a fundamental trade-off between closed-loop response time and overall bandwidth. 
        /// A minimal value, which is determined by <see cref="MaxReadFrameSize"/>, will may provide the lowest response latency,
        /// so long as data can be cleared form hardware memory fast enough to prevent buffering. Larger values will reduce system
        /// call frequency, increase overall bandwidth, and may improve processing performance for high-bandwidth data sources.
        /// The optimal value depends on the host computer and hardware configuration and must be determined via testing (e.g.
        /// using <see cref="MemoryMonitorData"/>).
        /// </remarks>
        public int BlockReadSize => ctx.BlockReadSize;

        /// <summary>
        /// Gets the number of bytes that are pre-allocated for writing data to hardware.
        /// </summary>
        /// <remarks>
        /// This value determines the amount of memory pre-allocated for calls to <see cref="oni.Context.Write(uint, IntPtr, int)"/>,
        /// <see cref="oni.Context.Write{T}(uint, T)"/>, and <see cref="oni.Context.Write{T}(uint, T[])"/>. A larger size will reduce
        /// the average amount of dynamic memory allocation system calls but increase the cost of each of those calls. The minimum
        /// size of this option is determined by <see cref="MaxWriteFrameSize"/>. The effect on real-timer performance is not as
        /// large as that of <see cref="BlockReadSize"/>.
        /// </remarks>
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
                    action();
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
                            ctx?.Dispose();
                            ctx = null;
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
