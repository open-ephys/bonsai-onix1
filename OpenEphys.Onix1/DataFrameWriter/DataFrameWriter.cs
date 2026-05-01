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
    public class DataFrameWriter : FileSink
    {
        const int SecondsBeforeFlush = 5;

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
            return new BufferedDataFrameArrowFileSink(TimeSpan.FromSeconds(SecondsBeforeFlush))
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            }.Process(source);
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
            return new DataFrameArrowFileSink(TimeSpan.FromSeconds(SecondsBeforeFlush))
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            }.Process(source);
        }
    }
}
