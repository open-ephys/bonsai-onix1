using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Builds and subscribes to one round's full acquisition-configuration pipeline for a headstage: a fresh
    /// <see cref="ContextTask"/>, at the given hardware address, running the given headstage's device tree
    /// through to <see cref="StartAcquisition"/>. Shared by the Survey and Scan runners, which otherwise
    /// duplicate nothing else about bringing hardware up.
    /// </summary>
    internal static class HeadstageConnection
    {
        /// <summary>
        /// Builds and subscribes to the pipeline. Disposing the returned subscription tears the <see
        /// cref="ContextTask"/> down completely before returning, so it's safe to build the next round's
        /// pipeline immediately afterward.
        /// </summary>
        /// <param name="headstage">The headstage to configure this round.</param>
        /// <param name="driver">Device driver used to communicate with the ONIX hardware controller.</param>
        /// <param name="hubIndex">Host interconnect index for the ONIX hardware controller.</param>
        /// <param name="validationLevel">How strictly hardware validation checks are enforced.</param>
        /// <param name="readSize">The <see cref="StartAcquisition.ReadSize"/> to acquire with.</param>
        /// <param name="onError">Called if the configuration pipeline fails.</param>
        internal static IDisposable Configure(MultiDeviceFactory<OnixContextTask> headstage, string driver, int hubIndex,
            ValidationLevel validationLevel, int readSize, Action<Exception> onError)
        {
            var pipeline = new StartAcquisition() { ReadSize = readSize}.Process(
                headstage.Process(new CreateContext { Driver = driver, Index = hubIndex, ValidationLevel = validationLevel }.Generate()));
            return pipeline.Subscribe(_ => { }, onError);
        }

        /// <summary>
        /// The device info registered under <paramref name="deviceName"/>, or <c>null</c> if there is none or it is
        /// not a <typeparamref name="T"/>. Call it while the context that registered the device is running.
        /// </summary>
        internal static T TryGetDeviceInfo<T>(string deviceName) where T : DeviceInfo
        {
            T result = null;
            DeviceManager.GetDevice(deviceName).Subscribe(info => result = info as T, _ => { });
            return result;
        }

        /// <summary>
        /// Describes the port voltage a round ran with, for failure messages.
        /// </summary>
        /// <param name="requested">The manually set voltage, or <c>null</c> if the voltage is auto-negotiated.</param>
        /// <param name="applied">The voltage actually applied, if known.</param>
        internal static string DescribePortVoltage(double? requested, double? applied)
        {
            if (requested.HasValue) return $"port voltage {requested:0.0} V (set manually)";
            if (applied.HasValue) return $"port voltage {applied:0.0} V (auto-negotiated)";
            return "port voltage auto-negotiated";
        }

        /// <summary>
        /// Hint logged with communication failures when the port voltage is auto-negotiated.
        /// </summary>
        internal const string AutoPortVoltageHint =
            "If this headstage needs a manually set port voltage (e.g. with long or spliced coaxial cables), " +
            "set it using Edit under Hardware configuration.";
    }
}
