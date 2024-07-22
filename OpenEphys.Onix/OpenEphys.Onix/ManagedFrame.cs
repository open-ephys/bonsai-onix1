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
            Sample = frame.GetData<T>();
            FrameClock = frame.Clock;
            DeviceAddress = frame.DeviceAddress;
        }

        /// <summary>
        /// Get the data pointed at by <see cref="oni.Frame.Data"/>.
        /// </summary>
        public readonly T[] Sample;

        /// <summary>
        /// Get the <see cref="oni.Frame.Clock"/> of the underlying frame.
        /// </summary>
        public ulong FrameClock { get; private set; }

        /// <summary>
        /// Get the <see cref="oni.Frame.DeviceAddress"/> of the underlying frame.
        /// </summary>
        public uint DeviceAddress { get; private set; }
    }
}
