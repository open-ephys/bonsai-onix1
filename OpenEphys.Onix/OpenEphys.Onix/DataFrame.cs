namespace OpenEphys.Onix
{
    public abstract class DataFrame
    {
        internal DataFrame(ulong clock)
        {
            Clock = clock;
        }

        internal DataFrame(ulong clock, ulong hubClock)
            : this(clock)
        {
            HubClock = hubClock;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; internal set; }
    }

    public abstract class BufferedDataFrame
    {
        internal BufferedDataFrame(ulong[] clock, ulong[] hubClock)
        {
            Clock = clock;
            HubClock = hubClock;
        }

        public ulong[] Clock { get; }

        public ulong[] HubClock { get; }
    }
}
