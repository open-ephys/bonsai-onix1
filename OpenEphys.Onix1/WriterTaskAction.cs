namespace OpenEphys.Onix1
{
    abstract class WriterTaskAction
    {
        private protected readonly uint deviceAddress;

        public WriterTaskAction(uint deviceAddress)
        {
            this.deviceAddress = deviceAddress;
        }

        internal abstract void Write(oni.Context ctx);
    }
}
