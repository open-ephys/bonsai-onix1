using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apache.Arrow;
using Apache.Arrow.Compression;
using Apache.Arrow.Ipc;
using OpenCV.Net;
using OpenEphys.Onix1.DataFrameWriter;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public class DataFrameWriterTests
    {
        const int Channels = 12;
        const int SamplesPerFrame = 4;

        // Batches of buffered frames of the same size are built over the same combined Mat, which is replaced when the
        // size changes, and disposed with the writer. The file still holds the data of every batch, so each batch was
        // serialized before the next one overwrote the Mat.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BufferedWriter_ReusesCombinedMat_UntilBatchSizeChanges(bool enableCompression)
        {
            var frames = Enumerable.Range(0, 5).Select(CreateFrame).ToList();
            var frameType = typeof(AnalogInputDataFrame);
            var fieldGroups = DataFrameWriterHelper.BuildFieldMappings(DataFrameWriterHelper.GetDataMembers(frameType), frames[0]);
            var schema = DataFrameWriterHelper.BuildSchema(fieldGroups);
            var builder = RecordBatchExpressionFactory.CreateBuilder<BufferedDataFrame>(
                new BufferedDataFrameExpressionProvider(), frameType, fieldGroups);
            var buffer = Assert.IsType<MatBuffer>(Assert.Single(builder.Resources));

            var combined = new List<Mat>();
            var recordingBuilder = new RecordBatchBuilder<BufferedDataFrame>((items, batchSchema) =>
            {
                var batch = builder.Build(items, batchSchema);
                combined.Add(buffer.Mat);
                return batch;
            }, builder);

            var fileName = Path.GetTempFileName();
            try
            {
                // NB: Batches of two, two and one, the last on dispose
                using (var writer = new ArrowBatchWriter<BufferedDataFrame>(fileName, schema, 2, TimeSpan.FromHours(1), recordingBuilder, enableCompression))
                {
                    foreach (var frame in frames)
                        writer.Write(frame);
                }

                Assert.Equal(3, combined.Count);
                Assert.Same(combined[0], combined[1]);
                Assert.NotSame(combined[1], combined[2]);
                Assert.All(combined, mat => Assert.True(mat.IsClosed, "A combined Mat was not disposed."));
                Assert.Null(buffer.Mat);
                Assert.All(frames, frame => Assert.False(frame.AnalogData.IsClosed, "A frame's own Mat was disposed."));

                var written = ReadColumns(fileName, schema);
                Assert.Equal(frames.SelectMany(frame => frame.Clock), written["Clock"].Select(value => (ulong)value));
                for (int channel = 0; channel < Channels; channel++)
                {
                    var expected = frames.SelectMany(frame => Enumerable.Range(0, SamplesPerFrame)
                        .Select(sample => (double)GetSample(frame.AnalogData, channel, sample)));
                    Assert.Equal(expected, written[$"AnalogData{channel}"]);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        static AnalogInputDataFrame CreateFrame(int index)
        {
            var clock = Enumerable.Range(0, SamplesPerFrame).Select(sample => (ulong)(index * SamplesPerFrame + sample)).ToArray();
            var data = new Mat(Channels, SamplesPerFrame, Depth.F32, 1);
            for (int channel = 0; channel < Channels; channel++)
                for (int sample = 0; sample < SamplesPerFrame; sample++)
                    data.SetReal(channel, sample, 1000 * index + 100 * channel + sample);
            return new AnalogInputDataFrame(clock, clock, data);
        }

        static float GetSample(Mat data, int channel, int sample) => (float)data.GetReal(channel, sample);

        static Dictionary<string, List<double>> ReadColumns(string fileName, Schema schema)
        {
            var columns = schema.FieldsList.ToDictionary(field => field.Name, _ => new List<double>());
            using var stream = File.OpenRead(fileName);
            using var reader = new ArrowFileReader(stream, new CompressionCodecFactory());
            RecordBatch batch;
            while ((batch = reader.ReadNextRecordBatch()) != null)
            {
                using (batch)
                {
                    foreach (var field in schema.FieldsList)
                    {
                        var values = columns[field.Name];
                        switch (batch.Column(field.Name))
                        {
                            case UInt64Array array: values.AddRange(array.Values.ToArray().Select(value => (double)value)); break;
                            case FloatArray array: values.AddRange(array.Values.ToArray().Select(value => (double)value)); break;
                            default: throw new NotSupportedException($"Unexpected column type for '{field.Name}'.");
                        }
                    }
                }
            }

            return columns;
        }
    }
}
