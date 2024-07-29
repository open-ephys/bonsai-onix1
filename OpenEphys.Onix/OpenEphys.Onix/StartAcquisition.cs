using System;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Starts data acquisition and frame distribution within a given <see cref="ContextTask"/>.
    /// </summary>
    public class StartAcquisition : Combinator<ContextTask, IGroupedObservable<uint, oni.Frame>>
    {
        /// <summary>
        /// Gets or sets the number of bytes read by the device driver access to the read channel.
        /// </summary>
        /// <remarks>
        /// This option allows control over a fundamental trade-off between closed-loop response time and overall bandwidth. 
        /// A minimal value, which is determined by <see cref="MaxReadFrameSize"/>, will may provide the lowest response latency,
        /// so long as data can be cleared form hardware memory fast enough to prevent buffering. Larger values will reduce system
        /// call frequency, increase overall bandwidth, and may improve processing performance for high-bandwidth data sources.
        /// The optimal value depends on the host computer and hardware configuration and must be determined via testing (e.g.
        /// using <see cref="MemoryMonitorData"/>).
        /// </remarks>
        public int ReadSize { get; set; } = 2048;

        /// <summary>
        /// Gets or sets the number of bytes that are pre-allocated for writing data to hardware.
        /// </summary>
        /// <remarks>
        /// This value determines the amount of memory pre-allocated for calls to <see cref="oni.Context.Write(uint, IntPtr, int)"/>,
        /// <see cref="oni.Context.Write{T}(uint, T)"/>, and <see cref="oni.Context.Write{T}(uint, T[])"/>. A larger size will reduce
        /// the average amount of dynamic memory allocation system calls but increase the cost of each of those calls. The minimum
        /// size of this option is determined by <see cref="MaxWriteFrameSize"/>. The effect on real-timer performance is not as
        /// large as that of <see cref="BlockReadSize"/>.
        /// </remarks>
        public int WriteSize { get; set; } = 2048;

        /// <summary>
        /// Accepts a sequence of <see cref="ContextTask"/> objects and returns a sequence of <see cref="oni.Frame"/> sequences,
        /// each of which are produced by a single device with one input context task.
        /// </summary>
        /// <param name="source">A sequence of <see cref="ContextTask"/> objects.</param>
        /// <returns>A sequence of <see cref="oni.Frame"/> sequences, each of which are produced by a single device with one input
        /// <see cref="ContextTask"/>.</returns>
        public override IObservable<IGroupedObservable<uint, oni.Frame>> Process(IObservable<ContextTask> source)
        {
            return source.SelectMany(context =>
            {
                return Observable.Create<IGroupedObservable<uint, oni.Frame>>((observer, cancellationToken) =>
                {
                    var frameSubscription = context.GroupedFrames.SubscribeSafe(observer);
                    try
                    {
                        return context.StartAsync(ReadSize, WriteSize, cancellationToken)
                                      .ContinueWith(_ => frameSubscription.Dispose());
                    }
                    catch
                    {
                        frameSubscription.Dispose();
                        throw;
                    }
                });
            });
        }
    }
}
