using Bonsai;
using System;
using System.ComponentModel;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Probe scope for a NeuropixelsV1 probe, offering the spike and LFP bands.
    /// </summary>
    public class NeuropixelsV1ProbeScopeVisualizer : ProbeScopeVisualizer<NeuropixelsV1DataFrame>
    {
        private protected override string DeviceNameOf(object upstreamOperator) =>
            upstreamOperator is NeuropixelsV1eData data ? data.DeviceName : null;

        private protected override ProbeScopeSource<NeuropixelsV1DataFrame> CreateSource(DeviceInfo info)
        {
            if (info is not NeuropixelsV1PsbDecoderDeviceInfo v1)
                throw new InvalidOperationException($"{info.DeviceType.Name} is not a NeuropixelsV1 probe this scope can display.");

            var configuration = v1.ProbeConfiguration
                ?? throw new InvalidOperationException("The device has no probe configuration, so its amplifier gain is unknown.");

            const int spikeRate = NeuropixelsV1.SamplesPerChannelPerSecond;
            const int lfpRate = NeuropixelsV1.SamplesPerChannelPerSecond / NeuropixelsV1.FramesPerRoundRobin;

            return new(v1.ProbeGroup, new[]
            {
                new ProbeScopeBand<NeuropixelsV1DataFrame>("Spike", spikeRate, frames =>
                    new NeuropixelsV1Scale
                    {
                        Band = NeuropixelsV1EphysBand.Spike,
                        AmplifierGain = configuration.SpikeAmplifierGain
                    }.Process(frames)),
                new ProbeScopeBand<NeuropixelsV1DataFrame>("LFP", lfpRate, frames =>
                    new NeuropixelsV1Scale
                    {
                        Band = NeuropixelsV1EphysBand.Lfp,
                        AmplifierGain = configuration.LfpAmplifierGain
                    }.Process(frames)),
            });
        }
    }

    /// <summary>
    /// Marks a point directly downstream of a <see cref="NeuropixelsV1eData"/> operator where a probe
    /// schematic and live waveform viewer can be opened.
    /// </summary>
    [TypeVisualizer(typeof(NeuropixelsV1ProbeScopeVisualizer))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Displays an interactive probe schematic beside live waveforms for a NeuropixelsV1 probe.")]
    public class NeuropixelsV1ProbeScopeBuilder : ProbeScopeBuilder<NeuropixelsV1DataFrame>
    {
    }
}
