namespace OpenEphys.Onix1.Tests
{
    /// <summary>
    /// A context with no hardware-specific checks, which opens any driver, including the ONI test driver.
    /// </summary>
    sealed class GenericContextTask : ContextTask
    {
        public GenericContextTask(string driver, int index)
            : base(driver, index)
        {
        }
    }
}
