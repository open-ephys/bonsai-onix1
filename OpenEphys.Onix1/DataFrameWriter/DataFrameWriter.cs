using System;
using Bonsai;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Represents an operator that writes each data frame in the sequence
    /// to an Apache Arrow file using an <see cref="ArrowWriter"/>.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class DataFrameWriter : FileSink, IArrowSinkOptions
    {
        const int SecondsBeforeFlush = 5;

        /// <summary>
        /// Gets or sets a value indicating whether to enable compression when writing to the Arrow file.
        /// </summary>
        public bool EnableCompression { get; set; } = false;

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
            return ConfigureSink(new BufferedDataFrameArrowFileSink(TimeSpan.FromSeconds(SecondsBeforeFlush))).Process(source);
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
            return ConfigureSink(new DataFrameArrowFileSink(TimeSpan.FromSeconds(SecondsBeforeFlush))).Process(source);
        }

        T ConfigureSink<T>(T sink) where T : FileSink, IArrowSinkOptions
        {
            sink.FileName = FileName;
            sink.Suffix = Suffix;
            sink.Buffered = Buffered;
            sink.Overwrite = Overwrite;
            sink.EnableCompression = EnableCompression;
            return sink;
        }
    }
}
