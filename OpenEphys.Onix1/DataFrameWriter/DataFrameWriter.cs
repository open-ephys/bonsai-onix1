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
        public const int SecondsBeforeFlush = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFrameWriter"/> class.
        /// </summary>
        public DataFrameWriter()
        {
            Buffered = true;
        }

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
        public bool Buffered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite { get; set; }

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

            Type sinkDefinition;
            if (typeof(BufferedDataFrame).IsAssignableFrom(elementType))
            {
                sinkDefinition = typeof(BufferedDataFrameArrowFileSink<>);
            }
            else if (typeof(DataFrame).IsAssignableFrom(elementType))
            {
                sinkDefinition = typeof(DataFrameArrowFileSink<>);
            }
            else
            {
                throw new WorkflowBuildException(
                    $"{nameof(DataFrameWriter)} requires a sequence of {nameof(DataFrame)} or " +
                    $"{nameof(BufferedDataFrame)} objects, but the input sequence has element type " +
                    $"'{elementType}'.", this);
            }

            var sinkType = sinkDefinition.MakeGenericType(elementType);
            var timeout = TimeSpan.FromSeconds(SecondsBeforeFlush);
            var sink = (FileSink)Activator.CreateInstance(sinkType, timeout);
            sink.FileName = FileName;
            sink.Suffix = Suffix;
            sink.Buffered = Buffered;
            sink.Overwrite = Overwrite;
            ((IArrowFileSink)sink).EnableCompression = EnableCompression;

            var processMethod = sinkType.GetMethod("Process", new[] { source.Type });
            var instance = Expression.Constant(sink);
            return Expression.Call(instance, processMethod, source);
        }
    }
}
