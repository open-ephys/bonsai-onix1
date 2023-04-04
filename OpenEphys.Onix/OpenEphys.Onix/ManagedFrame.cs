namespace OpenEphys.Onix
{
    /// <summary>
    /// Managed copy of <see cref="oni.Frame"/> with strongly-typed data array.
    /// </summary>
    /// <typeparam name="T">The data type of the Sample array</typeparam>
    public class ManagedFrame<T> where T : unmanaged
    {
        public ManagedFrame(oni.Frame frame)
        {
            Sample = frame.Data<T>();
            FrameClock = frame.Clock;
            DeviceAddress = frame.DeviceAddress;
        }

        public readonly T[] Sample;

        public ulong FrameClock { get; private set; }

        public uint DeviceAddress { get; private set; }
    }
}
