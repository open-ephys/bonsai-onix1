namespace OpenEphys.Onix1
{
    sealed class ScalarWriterTaskAction<T> : WriterTaskAction where T : unmanaged
    {
        readonly T data;

        public ScalarWriterTaskAction(uint deviceAddress, T data) : base(deviceAddress)
        {
            this.data = data;
        }

        internal override void Write(oni.Context ctx) => ctx.Write(deviceAddress, data);
    }
}
