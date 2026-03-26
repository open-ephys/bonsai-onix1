using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1.FrameWriter;

[Description("Generates synthetic data.")]
[WorkflowElementCategory(ElementCategory.Source)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class GenerateSyntheticData : Source<NeuropixelsV1DataFrame>
{
    private const int ChannelCount = 384;

    [Description("Number of samples per frame (columns in the 384 x N Mat).")]
    [DefaultValue(36)]
    public int SamplesPerFrame { get; set; } = 36;

    [Description("Total number of frames to emit before the sequence completes.")]
    [DefaultValue(1000)]
    public int NumberOfFrames { get; set; } = 1000;

    public override IObservable<NeuropixelsV1DataFrame> Generate()
    {
        const int dataRatio = 12;
        const ushort midpoint = 512;
        const int maxOffsetFromMid = 50;

        var samples = SamplesPerFrame;
        var total = NumberOfFrames;

        var rng = new Random();

        var channelOffsets = new ushort[ChannelCount];
        for (int i = 0; i < ChannelCount; i++)
        {
            channelOffsets[i] = (ushort)(midpoint + rng.Next(-maxOffsetFromMid, maxOffsetFromMid + 1));
        }

        ushort[,] apData = new ushort[ChannelCount, samples];
        ushort[,] lfpData = new ushort[ChannelCount, samples / dataRatio];
        ulong[] clock = new ulong[samples];
        int[] frameCount = new int[samples];

        for (int j = 0; j < samples; j++)
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                var noise = (ushort)rng.Next(-50, 51);
                ushort value = (ushort)(channelOffsets[i] + noise);

                apData[i, j] = value;

                if (j % dataRatio == 0)
                {
                    lfpData[i, j / dataRatio] = value;
                }
            }
        }

        var matApData = Mat.FromArray(apData);
        var matLfpData = Mat.FromArray(lfpData);

        var clockOffset = 0;
        var frameCountOffset = 0;

        return Observable
            .Range(0, total)
            .Select(_ =>
            {
                for (int i = 0; i < samples; i++)
                {
                    clock[i] = (ulong)(i + clockOffset);
                }

                clockOffset += samples;

                for (int i = 0; i < samples; i++)
                {
                    frameCount[i] = i + frameCountOffset;
                }

                frameCountOffset += samples;

                var frame = new NeuropixelsV1DataFrame(clock, clock, frameCount, matApData, matLfpData);

                return frame;
            });
    }
}
