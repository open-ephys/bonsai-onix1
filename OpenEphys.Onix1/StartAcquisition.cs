using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Starts data acquisition and frame distribution on a <see cref="ContextTask"/>.
    /// </summary>
    [Description("Starts data acquisition and frame distribution on a ContextTask.")]
    public class StartAcquisition : Combinator<ContextTask, IGroupedObservable<uint, oni.Frame>>
    {
        /// <summary>
        /// Gets or sets the number of bytes read per cycle of the <see cref="ContextTask"/>'s acquisition thread.
        /// </summary>
        /// <remarks>
        /// This option allows control over a fundamental trade-off between closed-loop response time and overall bandwidth. 
        /// A minimal value, which is determined by <see cref="ContextTask.MaxReadFrameSize"/>, will provide the lowest response latency,
        /// so long as data can be cleared from hardware memory fast enough to prevent buffering. Larger values will reduce system
        /// call frequency, increase overall bandwidth, and may improve processing performance for high-bandwidth data sources.
        /// The optimal value depends on the host computer and hardware configuration and must be determined via testing (e.g.
        /// using <see cref="MemoryMonitorData"/>).
        /// </remarks>
        [Description("Number of bytes read per cycle of the acquisition thread.")]
        public int ReadSize { get; set; } = 2048;

        /// <summary>
        /// Gets or sets the number of bytes that are pre-allocated for writing data to hardware.
        /// </summary>
        /// <remarks>
        /// This value determines the amount of memory pre-allocated for calls to <see cref="oni.Context.Write(uint, IntPtr, int)"/>,
        /// <see cref="oni.Context.Write{T}(uint, T)"/>, and <see cref="oni.Context.Write{T}(uint, T[])"/>. A larger size will reduce
        /// the average amount of dynamic memory allocation system calls but increase the cost of each of those calls. The minimum
        /// size of this option is determined by <see cref="ContextTask.MaxWriteFrameSize"/>. The effect on real-timer performance is not as
        /// large as that of <see cref="ContextTask.BlockReadSize"/>.
        /// </remarks>
        [Description("The number of bytes that are pre-allocated for writing data to hardware.")]
        public int WriteSize { get; set; } = 2048;

        /// <summary>
        /// Starts data acquisition and frame distribution on a <see cref="ContextTask"/> and returns
        /// the sequence of all received <see cref="oni.Frame"/> objects, grouped by device address.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ContextTask"/> objects on which to start data acquisition
        /// and frame distribution.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="oni.Frame"/> objects for each <see cref="ContextTask"/>,
        /// grouped by device address.
        /// </returns>
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
