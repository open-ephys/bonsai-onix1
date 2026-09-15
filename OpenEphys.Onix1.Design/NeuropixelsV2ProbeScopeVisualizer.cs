using Bonsai;
using System;
using System.ComponentModel;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Probe scope for a NeuropixelsV2 probe, which has a single wideband output.
    /// </summary>
    public class NeuropixelsV2ProbeScopeVisualizer : ProbeScopeVisualizer<NeuropixelsV2DataFrame>
    {
        private protected override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV2eData data ? data.DeviceName : null;

        private protected override ProbeScopeSource<NeuropixelsV2DataFrame> CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV2PsbDecoderDeviceInfo v2)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV2 probe this scope can display.");

            return new(v2.ProbeGroup, new[]
            {
                new ProbeScopeBand<NeuropixelsV2DataFrame>("Wideband", NeuropixelsV2.SamplesPerChannelPerSecond,
                    frames => new NeuropixelsV2Scale().Process(frames)),
            });
        }
    }

    /// <summary>
    /// Marks a point directly downstream of a <see cref="NeuropixelsV2eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    [TypeVisualizer(typeof(NeuropixelsV2ProbeScopeVisualizer))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV2 probe.")]
    public class NeuropixelsV2ProbeScopeBuilder : ProbeScopeBuilder<NeuropixelsV2DataFrame>
    {
    }
}
