using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEphys.Onix
{
    public class ContextTask : IDisposable
    {
        private oni.Context ctx;

        /// <summary>
        /// Maximum amount of frames the reading queue will hold. If the queue fills or the read
        /// thread is not performant enough to fill it faster than data is produced, frame reading
        /// will throttle, filling host memory instead of userspace memory.
        /// </summary>
        private const int MaxQueuedFrames = 2_000_000;

        /// <summary>
        /// Timeout in ms for queue reads. This should not be critical as the read operation will
        /// cancel if the task is stopped
        /// </summary>
        private const int QueueTimeoutMilliseconds = 200;

        /// <summary>
        /// In this package most operators are tied in to the RIFFA PCIe backend used by the FMC host.
        /// </summary>
        internal const string DefaultDriver = "riffa";
        internal const int DefaultIndex = 0;

        // NB: Decouple OnNext() form hadware reads
        private Task readFrames;
        private Task distributeFrames;
        private BlockingCollection<oni.Frame> FrameQueue;
        private CancellationTokenSource CollectFramesTokenSource;
        private IDisposable ContextConfiguration;
        event Func<ContextTask, IDisposable> configureHost;
        event Func<ContextTask, IDisposable> configureLink;
        event Func<ContextTask, IDisposable> configureDevice;

        // FrameReceived observable sequence
        internal Subject<oni.Frame> FrameReceived = new();

        // TODO: These work for RIFFA implementation, but potentially not others!!
        private readonly object readLock = new();
        private readonly object writeLock = new();
        private readonly object regLock = new();
        private readonly object disposeLock = new();
        private bool running = false;

        private readonly string contextDriver = DefaultDriver;
        private readonly int contextIndex = DefaultIndex;

        public ContextTask(string driver, int index)
        {
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

        public void Reset()
        {
            lock (disposeLock)
                lock (regLock)
                {
                    Stop();
                    lock (readLock)
                        lock (writeLock)
                        {
                            ctx?.Dispose();
                            Initialize();
                        }
                }
        }

        public uint SystemClockHz { get; private set; }

        public uint AcquisitionClockHz { get; private set; }

        public uint MaxReadFrameSize { get; private set; }

        public uint MaxWriteFrameSize { get; private set; }

        public Dictionary<uint, oni.Device> DeviceTable { get; private set; }

        void AssertConfigurationContext()
        {
            if (running)
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
                configureHost += configure;
            }
        }

        // NB: This is where actions that calibrate port voltage or otherwise
        // check link lock state should be executed
        internal void ConfigureLink(Func<ContextTask, IDisposable> configure)
        {
            lock (regLock)
            {
                AssertConfigurationContext();
                configureLink += configure;
            }
        }

        // NB: Actions queued using this method should assume that the device table
        // is finalized and cannot be changed
        internal void ConfigureDevice(Func<ContextTask, IDisposable> configure)
        {
            lock (regLock)
            {
                AssertConfigurationContext();
                configureDevice += configure;
            }
        }

        private IDisposable ConfigureContext()
        {
            var hostAction = Interlocked.Exchange(ref configureHost, null);
            var linkAction = Interlocked.Exchange(ref configureLink, null);
            var deviceAction = Interlocked.Exchange(ref configureDevice, null);
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

        internal void Start(int blockReadSize, int blockWriteSize)
        {
            lock (regLock)
            {
                if (running) return;

                // NB: Configure context before starting acquisition
                ContextConfiguration = ConfigureContext();
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

                CollectFramesTokenSource = new CancellationTokenSource();
                var collectFramesToken = CollectFramesTokenSource.Token;

                FrameQueue = new BlockingCollection<oni.Frame>(MaxQueuedFrames);

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
                            oni.Frame frame = ReadFrame();
                            FrameQueue.Add(frame, collectFramesToken);

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
                            if (FrameQueue.TryTake(out oni.Frame frame, QueueTimeoutMilliseconds, collectFramesToken))
                            {
                                FrameReceived.OnNext(frame);
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

                running = true;
            }
        }

        internal void Stop()
        {
            lock (regLock)
            {
                if (!running) return;
                if ((distributeFrames != null || readFrames != null) && !distributeFrames.IsCanceled)
                {
                    CollectFramesTokenSource.Cancel();
                    Task.WaitAll(new Task[] { distributeFrames, readFrames });
                }
                CollectFramesTokenSource?.Dispose();
                CollectFramesTokenSource = null;

                // Clear queue and free memory
                while (FrameQueue?.Count > 0)
                {
                    var frame = FrameQueue.Take();
                    frame.Dispose();
                }
                FrameQueue?.Dispose();
                FrameQueue = null;
                ctx.Stop();
                running = false;

                ContextConfiguration?.Dispose();
            }
        }

        #region oni.Context Properties

        internal bool Running => ctx.Running;

        public int HardwareAddress
        {
            get => ctx.HardwareAddress;
            set => ctx.HardwareAddress = value;
        }

        public int BlockReadSize => ctx.BlockReadSize;

        public int BlockWriteSize => ctx.BlockWriteSize;

        // PortA and PortB each have a bit in portfunc
        public PassthroughState HubState
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
                if (ctx != null)
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

        public oni.Frame ReadFrame()
        {
            lock (readLock)
            {
                return ctx.ReadFrame();
            }
        }

        public void Write<T>(uint deviceAddress, T data) where T : unmanaged
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data);
            }
        }

        public void Write<T>(uint deviceAddress, T[] data) where T : unmanaged
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data);
            }
        }

        public void Write(uint deviceAddress, IntPtr data, int dataSize)
        {
            lock (writeLock)
            {
                ctx.Write(deviceAddress, data, dataSize);
            }
        }

        public oni.Hub GetHub(uint deviceAddress) => ctx.GetHub(deviceAddress);

        public virtual uint GetPassthroughDeviceAddress(uint deviceAddress)
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

        public void Dispose()
        {
            lock (disposeLock)
                lock (regLock)
                {
                    Stop();
                    lock (readLock)
                        lock (writeLock)
                        {
                            ctx?.Dispose();
                            ctx = null;
                        }
                }

            GC.SuppressFinalize(this);
        }
    }
}
