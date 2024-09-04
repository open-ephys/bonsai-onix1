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
    /// <remarks>
    /// The <see href="https://open-ephys.github.io/ONI/">open neuro interface (ONI)</see> hardware
    /// specification and API describe a general purpose acquisition system architecture and programming
    /// interface for communication with a host PC. One requirement of ONI is a sequence of events that must
    /// occur in order to start synchronized data acquisition. <see cref="StartAcquisition"/> performs these
    /// required actions on one or more <see cref="ContextTask">ContextTasks</see> provided in its input
    /// sequence. Once acquisition is started, devices managed by a particular <see cref="ContextTask"/> will
    /// start to produce data in a format called an <see
    /// href="https://open-ephys.github.io/ONI/api/liboni/oni.html#c.oni_frame_t">ONI frame</see>. The output
    /// sequence of this operator is therefore a <see cref="IGroupedObservable{TKey, TElement}"/>, where
    /// <list type="table">
    /// <item>
    /// <term>Tkey</term>
    /// <description>
    /// Is the address of a particular hardware device within a single <see cref="ContextTask"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <term>TElement</term>
    /// <description>
    /// Is a ONI Frame produced by the device with address Tkey.
    /// </description>
    /// </item>
    /// </list>
    /// These pre-sorted frame sequences can be interpreted by downstream Data I/O operators (e.g. <see
    /// cref="BreakoutAnalogInput"/> or <see cref="Bno055Data"/>) that convert ONI frames, which consist of
    /// byte arrays with a header and data block, into data types that are are more amenable to processing
    /// within Bonsai workflows.
    /// </remarks>
    [Description("Starts data acquisition and frame distribution on a ContextTask.")]
    public class StartAcquisition : Combinator<ContextTask, IGroupedObservable<uint, oni.Frame>>
    {
        /// <summary>
        /// Gets or sets the number of bytes read per cycle of the <see cref="ContextTask"/>'s acquisition
        /// thread.
        /// </summary>
        /// <remarks>
        /// This option allows control over a fundamental trade-off between closed-loop response time and
        /// available bandwidth. A minimal value, which is determined by <see
        /// cref="ContextTask.MaxReadFrameSize"/>, will provide the lowest response latency, so long as data
        /// can be cleared from hardware memory fast enough to prevent buffering. Larger values will both
        /// reduce system call frequency and reduce the number of function calls per unit time performed by
        /// Bonsai, and therefore, increase available bandwidth. Larger values may improve processing
        /// performance for high-bandwidth data sources. The optimal value depends on the host computer and
        /// hardware configuration and must be determined via testing (e.g. using <see
        /// cref="MemoryMonitorData"/>).
        /// </remarks>
        [Description("Number of bytes read per cycle of the acquisition thread.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int ReadSize { get; set; } = 2048;

        /// <summary>
        /// Gets or sets the number of bytes that are pre-allocated for writing data to hardware.
        /// </summary>
        /// <remarks>
        /// This value determines the amount of memory that is pre-allocated for calls to <see
        /// cref="oni.Context.Write(uint, IntPtr, int)"/>, <see cref="oni.Context.Write{T}(uint, T)"/>, and
        /// <see cref="oni.Context.Write{T}(uint, T[])"/>. A larger size will reduce the frequency of
        /// dynamic memory allocation system calls but increase the expense of each of those calls. The minimum
        /// size of this option is determined by <see cref="ContextTask.MaxWriteFrameSize"/>. The effect on
        /// real-time performance is typically not as large as that of <see cref="ContextTask.BlockReadSize"/>
        /// because this parameter defines a readily-available pool of memory for the creation of output data
        /// frames, but does not determine when they are written to hardware. Data is written to hardware as
        /// soon as an output frame has been created. In contrast data is read from hardware whenever more than
        /// <see cref="ReadSize"/> bytes have accumulated in the input buffer.
        /// </remarks>
        [Description("The number of bytes that are pre-allocated for writing data to hardware.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int WriteSize { get; set; } = 2048;

        /// <summary>
        /// Starts data acquisition and frame distribution on a <see cref="ContextTask"/> and returns the
        /// sequence of all received <see cref="oni.Frame"/> objects, grouped by device address.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ContextTask"/> objects on which to start data acquisition and frame
        /// distribution.
        /// </param>
        /// <returns>
        /// A sequence of data frames produced by each <see cref="ContextTask"/> in the input sequence and
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
