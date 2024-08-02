namespace OpenEphys.Onix1
{
    /// <summary>
    /// An abstract class for representing <see cref="oni.Frame"/> objects in way that suits their use in this library.
    /// </summary>
    public abstract class DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFrame"/> class.
        /// </summary>
        /// <param name="clock">Acquisition clock count. Generally provided by the underlying <see cref="oni.Frame.Clock"/> value.</param>
        internal DataFrame(ulong clock)
        {
            Clock = clock;
        }

        internal DataFrame(ulong clock, ulong hubClock)
            : this(clock)
        {
            HubClock = hubClock;
        }

        /// <summary>
        /// Gets the acquisition clock count.
        /// </summary>
        /// <remarks>
        /// Acquisition clock count that is synchronous for all frames collected within an ONI context created using <see cref="CreateContext"/>.
        /// The acquisition clock rate is given by <see cref="ContextTask.AcquisitionClockHz"/>. This clock value provides a common, synchronized
        /// time base for all data collected with an single ONI context.
        /// </remarks>
        public ulong Clock { get; }

        /// <summary>
        /// Gets the hub clock count.
        /// </summary>
        /// <remarks>
        /// Local, potentially asynchronous, clock count. Aside from the synchronous <see cref="Clock"/> value, data frames also contain a local clock
        /// count produced within the <see cref="oni.Hub"/> that the data was actually produced within. For instance, a headstage may contain an onboard controller
        /// for controlling devices and arbitrating data stream that runs asynchronously from the <see cref="ContextTask.AcquisitionClockHz"/>. This value
        /// is therefore the most precise way to compare the sample time of data collected within a given <see cref="oni.Hub"/>. However, the delay between time of
        /// data collection and synchronous time stamping by <see cref="Clock"/> is very small (sub-microsecond) and this value can therefore
        /// be disregarded in most scenarios in favor of <see cref="Clock"/>.
        /// </remarks>
        public ulong HubClock { get; internal set; }
    }

    /// <summary>
    /// An abstract class for representing buffered groups <see cref="oni.Frame"/> objects in way that suits their use in this library.
    /// </summary>
    public abstract class BufferedDataFrame
    {
        internal BufferedDataFrame(ulong[] clock, ulong[] hubClock)
        {
            Clock = clock;
            HubClock = hubClock;
        }

        /// <summary>
        /// Gets the buffered array of <see cref="DataFrame.Clock"/> values.
        /// </summary>
        public ulong[] Clock { get; }

        /// <summary>
        /// Gets the buffered array of <see cref="DataFrame.HubClock"/> values.
        /// </summary>
        public ulong[] HubClock { get; }
    }
}
