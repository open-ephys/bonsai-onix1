namespace OpenEphys.Onix1
{
    sealed class ArrayWriterTaskAction<T> : WriterTaskAction where T : unmanaged
    {
        readonly T[] data;

        public ArrayWriterTaskAction(uint deviceAddress, T[] data) : base(deviceAddress)
        {
            this.data = data;
        }

        internal override void Write(oni.Context ctx) => ctx.Write(deviceAddress, data);
    }
}
