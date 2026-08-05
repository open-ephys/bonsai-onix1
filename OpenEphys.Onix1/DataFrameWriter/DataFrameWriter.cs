using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Bonsai;
using Bonsai.Expressions;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Represents an operator that writes each data frame in the sequence to an Apache Arrow file using an
    /// <see cref="ArrowWriter"/>.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class DataFrameWriter : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Maximum time before data is flushed to file even if internal buffer is not yet filled.
        /// </summary>
        const int SecondsBeforeFlush = 5;

        /// <summary>
        /// Gets or sets the name of the file on which to write the elements.
        /// </summary>
        [Description("The name of the file on which to write the elements.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the suffix used to generate file names.
        /// </summary>
        [Description("The suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <see langword="true"/>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to enable compression when writing to the Arrow file.
        /// </summary>
        /// <remarks>
        /// When enabled, data is compressed using the <see
        /// href="https://github.com/facebook/zstd">Zstandard</see> compression algorithm before
        /// writing. This reduces file size at the cost of additional CPU overhead on read and
        /// write.
        /// </remarks>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Constructs an expression that writes all of the data frames in the sequence to an Apache Arrow
        /// file.
        /// </summary>
        /// <param name="arguments">
        /// A collection containing a single <see cref="Expression"/> node representing the sequence of data
        /// frames to write.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> that, when compiled, returns an observable sequence identical to the
        /// input sequence, including its element type, but where there is an additional side effect of
        /// writing each frame to an Apache Arrow file.
        /// </returns>
        /// <exception cref="WorkflowBuildException">
        /// Thrown if upstream sequence is not composed of <see cref="DataFrame">DataFrames</see> or <see
        /// cref="BufferedDataFrame">BufferedDataFrames</see>.
        /// </exception>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var elementType = source.Type.GetGenericArguments()[0];
            string writeMethodName;
            if (typeof(BufferedDataFrame).IsAssignableFrom(elementType))
            {
                writeMethodName = nameof(WriteBuffered);
            }
            else if (typeof(DataFrame).IsAssignableFrom(elementType))
            {
                writeMethodName = nameof(Write);
            }
            else
            {
                throw new WorkflowBuildException(
                    $"{nameof(DataFrameWriter)} requires a sequence of {nameof(DataFrame)} or " +
                    $"{nameof(BufferedDataFrame)} objects, but the input sequence has element type " +
                    $"'{elementType}'.", this);
            }
            var instance = Expression.Constant(this);
            var writeMethod = typeof(DataFrameWriter)
                .GetMethod(writeMethodName)
                .MakeGenericMethod(elementType);
            return Expression.Call(writeMethod, source,
                Expression.Property(instance, nameof(FileName)),
                Expression.Property(instance, nameof(Suffix)),
                Expression.Property(instance, nameof(Buffered)),
                Expression.Property(instance, nameof(Overwrite)),
                Expression.Property(instance, nameof(EnableCompression)));
        }

        /// <summary>
        /// Writes all of the data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <typeparam name="TSource">The concrete <see cref="DataFrame"/> type in the sequence.</typeparam>
        /// <param name="source">The sequence of data frames to write.</param>
        /// <param name="fileName">The name of the file on which to write the elements.</param>
        /// <param name="suffix">The suffix used to generate file names.</param>
        /// <param name="buffered">Indicates whether writing should be buffered.</param>
        /// <param name="overwrite">Indicates whether to overwrite the output file if it already exists.</param>
        /// <param name="enableCompression">Indicates whether to enable compression when writing to the Arrow file.</param>
        /// <returns>
        /// An observable sequence identical to <paramref name="source"/>, including its element type, but where
        /// there is an additional side effect of writing each frame to an Apache Arrow file.
        /// </returns>
        public static IObservable<TSource> Write<TSource>(
            IObservable<TSource> source,
            string fileName,
            PathSuffix suffix = PathSuffix.None,
            bool buffered = true,
            bool overwrite = false,
            bool enableCompression = false)
            where TSource : DataFrame
        {
            return new DataFrameArrowFileSink<TSource>(TimeSpan.FromSeconds(SecondsBeforeFlush))
            {
                FileName = fileName,
                Suffix = suffix,
                Buffered = buffered,
                Overwrite = overwrite,
                EnableCompression = enableCompression
            }.Process(source);
        }

        /// <summary>
        /// Writes all of the buffered data frames in the sequence to an Apache Arrow file.
        /// </summary>
        /// <typeparam name="TSource">The concrete <see cref="BufferedDataFrame"/> type in the sequence.</typeparam>
        /// <param name="source">The sequence of buffered data frames to write.</param>
        /// <param name="fileName">The name of the file on which to write the elements.</param>
        /// <param name="suffix">The suffix used to generate file names.</param>
        /// <param name="buffered">Indicates whether writing should be buffered.</param>
        /// <param name="overwrite">Indicates whether to overwrite the output file if it already exists.</param>
        /// <param name="enableCompression">Indicates whether to enable compression when writing to the Arrow file.</param>
        /// <returns>
        /// An observable sequence identical to <paramref name="source"/>, including its element type, but where
        /// there is an additional side effect of writing each frame to an Apache Arrow file.
        /// </returns>
        public static IObservable<TSource> WriteBuffered<TSource>(
            IObservable<TSource> source,
            string fileName,
            PathSuffix suffix = PathSuffix.None,
            bool buffered = true,
            bool overwrite = false,
            bool enableCompression = false)
            where TSource : BufferedDataFrame
        {
            return new BufferedDataFrameArrowFileSink<TSource>(TimeSpan.FromSeconds(SecondsBeforeFlush))
            {
                FileName = fileName,
                Suffix = suffix,
                Buffered = buffered,
                Overwrite = overwrite,
                EnableCompression = enableCompression
            }.Process(source);
        }
    }
}
