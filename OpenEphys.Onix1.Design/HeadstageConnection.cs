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
        /// <param name="onError">Called if the configuration pipeline fails.</param>
        internal static IDisposable Configure(MultiDeviceFactory headstage, string driver, int hubIndex, Action<Exception> onError)
        {
            var pipeline = new StartAcquisition().Process(
                headstage.Process(new CreateContext { Driver = driver, Index = hubIndex }.Generate()));
            return pipeline.Subscribe(_ => { }, onError);
        }
    }
}
