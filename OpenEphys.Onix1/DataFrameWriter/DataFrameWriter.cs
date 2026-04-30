using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using Apache.Arrow;
using Bonsai;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Represents an operator that writes each data frame in the sequence
    /// to an Apache Arrow file using an <see cref="ArrowWriter"/>.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class DataFrameWriter : FileSink
    {
        /// <summary>
        /// Represents the number of seconds to wait before data is flushed, regardless of if <see
        /// cref="MinimumBufferSize"/> samples have been collected.
        /// </summary>
        const int SecondsBeforeFlush = 5;

        /// <summary>
        /// Represents the default number of <see cref="DataFrame">DataFrames</see> in a <see
        /// cref="RecordBatch"/>.
        /// </summary>
        const int DefaultBufferSize = 1000;

        /// <summary>
        /// Represents the mimumum number of <see cref="DataFrame">DataFrames</see> in a <see
        /// cref="RecordBatch"/>.
        /// </summary>
        const int MinimumBufferSize = 100;

        BufferedDataFrameArrowFileSink CreateBufferedDataFrameArrowFileSink(
            Schema schema,
            Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize,
            TimeSpan timeout)
        {
            return new BufferedDataFrameArrowFileSink(schema, createRecordBatch, bufferSize, timeout)
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        DataFrameArrowFileSink CreateDataFrameArrowFileSink(
            Schema schema,
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize,
            TimeSpan timeout)
        {
            return new DataFrameArrowFileSink(schema, createRecordBatch, bufferSize, timeout)
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        static Expression<Func<IList<DataFrame>, Schema, RecordBatch>> CreateDataFrameRecordBatchBuilder(
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            return RecordBatchExpressionFactory.CreateBuilder<Func<IList<DataFrame>, Schema, RecordBatch>>(
                new DataFrameExpressionProvider(),
                frameType,
                members);
        }

        static Expression<Func<IList<BufferedDataFrame>, Schema, RecordBatch>> CreateBufferedFrameRecordBatchBuilder(
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            return RecordBatchExpressionFactory.CreateBuilder<Func<IList<BufferedDataFrame>, Schema, RecordBatch>>(
                new BufferedDataFrameExpressionProvider(),
                frameType,
                members);
        }

        static int GetBufferSize(Type frameType)
        {
            var sampleRateAttribute = frameType.GetCustomAttribute<ExpectedSampleRateAttribute>();
            if (sampleRateAttribute != null)
            {
                const double BufferDurationSeconds = 1.0;
                var bufferSize = (int)(sampleRateAttribute.SampleRateHz * BufferDurationSeconds);
                return bufferSize >= MinimumBufferSize ? bufferSize : MinimumBufferSize;
            }

            return DefaultBufferSize;
        }

        /// <summary>
        /// Writes all of the data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <param name="source">The sequence of <see cref="BufferedDataFrame">BufferedDataFrame's</see> to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the frames to an Apache Arrow file.
        /// </returns>
        public IObservable<BufferedDataFrame> Process(IObservable<BufferedDataFrame> source)
        {
            return source.Replay(frames =>
            {
                var config = frames.Take(1).Select(frame =>
                {
                    var frameType = frame.GetType();
                    var members = DataFrameWriterHelper.GetDataMembers(frameType);
                    var bufferSize = (int)Math.Ceiling((double)GetBufferSize(frameType) / frame.Clock.Length);
                    var schema = DataFrameWriterHelper.GenerateSchema(members, frame);
                    var createRecordBatch = CreateBufferedFrameRecordBatchBuilder(frameType, members).Compile();
                    return (schema, createRecordBatch, bufferSize);
                });

                return config.SelectMany(c => CreateBufferedDataFrameArrowFileSink(c.schema, c.createRecordBatch, c.bufferSize, 
                    TimeSpan.FromSeconds(SecondsBeforeFlush)).Process(frames));
            }, 1);
        }

        /// <summary>
        /// Writes all of the data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <param name="source">The sequence of <see cref="DataFrame">DataFrame's</see> to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the frames to an Apache Arrow file.
        /// </returns>
        public IObservable<DataFrame> Process(IObservable<DataFrame> source)
        {
            return source.Replay(frames =>
            {
                var config = frames.Take(1).Select(frame =>
                {
                    var frameType = frame.GetType();
                    var members = DataFrameWriterHelper.GetDataMembers(frameType);
                    var bufferSize = GetBufferSize(frameType);
                    var schema = DataFrameWriterHelper.GenerateSchema(members, frame);
                    var createRecordBatch = CreateDataFrameRecordBatchBuilder(frameType, members).Compile();
                    return (schema, createRecordBatch, bufferSize);
                });

                return config.SelectMany(c => CreateDataFrameArrowFileSink(c.schema, c.createRecordBatch, c.bufferSize, 
                    TimeSpan.FromSeconds(SecondsBeforeFlush)).Process(frames));
            }, 1);
        }
    }
}
