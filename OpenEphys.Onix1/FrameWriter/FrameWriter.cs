using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using Apache.Arrow;
using Bonsai;
using Bonsai.IO;
using OpenCV.Net;

namespace OpenEphys.Onix1.FrameWriter
{
    /// <summary>
    /// Represents an operator that writes each data frame in the sequence
    /// to an Apache Arrow file using an <see cref="ArrowWriter"/>.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class FrameWriter : FileSink
    {
        BufferedDataFrameSink CreateBufferedDataFrameSink(
            Schema schema,
            Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize)
        {
            return new BufferedDataFrameSink(schema, createRecordBatch, bufferSize)
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        DataFrameSink CreateDataFrameSink(
            Schema schema,
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize)
        {
            return new DataFrameSink(schema, createRecordBatch, bufferSize)
            {
                FileName = this.FileName,
                Suffix = this.Suffix,
                Buffered = this.Buffered,
                Overwrite = this.Overwrite
            };
        }

        static object GetMemberValue(MemberInfo member, object instance)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(instance),
                PropertyInfo propertyInfo => propertyInfo.GetValue(instance),
                _ => throw new InvalidOperationException($"Cannot get value of {member.GetType()} member from {instance.GetType()} object."),
            };
        }

        static Schema GenerateSchema(IEnumerable<MemberInfo> members, object instance)
        {
            var fields = new List<Field>();
            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = FrameWriterHelper.GetMemberType(current.Member);

                if (memberType.IsPrimitive)
                {
                    fields.Add(new Field(current.GetFullName(), FrameWriterHelper.GetArrowType(memberType), false));
                }
                else if (memberType.IsArray)
                {
                    fields.Add(new Field(current.GetFullName(), FrameWriterHelper.GetArrowType(memberType.GetElementType()), false));
                }
                else if (memberType.IsEnum)
                {
                    // TODO: See if the Dictionary type in Arrow would be better for enums
                    fields.Add(new Field(current.GetFullName(), FrameWriterHelper.GetArrowType(Enum.GetUnderlyingType(memberType)), false));
                }
                else if (memberType.IsValueType)
                {
                    var structMembers = FrameWriterHelper.GetDataMembers(memberType);

                    foreach (var structMember in structMembers.Reverse())
                    {
                        if (FrameWriterHelper.IsMemberIgnored(current.Member, structMember))
                            continue;

                        stack.Push(new MemberNode
                        {
                            Member = structMember,
                            Parent = current
                        });
                    }
                }
                else if (memberType == typeof(Mat))
                {
                    var mat = GetMemberValue(current.Member, instance) as Mat ?? throw new NullReferenceException($"No valid Mat property on the {instance.GetType()} object.");

                    for (int i = 0; i < mat.Rows; i++)
                    {
                        // Note: Could add an attribute to data frames properties to specify custom field naming
                        fields.Add(new Field($"{current.GetFullName()}Ch{i}", FrameWriterHelper.GetArrowType(mat.Depth), false));
                    }
                }
                else
                {
                    throw new NotSupportedException($"The member type '{memberType}' is not supported for generating schemas.");
                }
            }

            return new Schema(fields, null);
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
            const int BufferSizeInSamples = 30000;
            Schema schema = null;
            Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch = null;
            int bufferSize = 1000;

            return source.Replay(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = FrameWriterHelper.GetDataMembers(frameType);
                        bufferSize = (int)Math.Ceiling((double)BufferSizeInSamples / frame.Clock.Length);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateBufferedFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Observable.Defer(() => CreateBufferedDataFrameSink(schema, createRecordBatch, bufferSize).Process(frames))
            ), 1);
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
            const int BufferSize = 50;
            Schema schema = null;
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch = null;

            return source.Replay(frames => Observable.Concat(
                frames.Take(1)
                    .Do(frame =>
                    {
                        var frameType = frame.GetType();
                        var members = FrameWriterHelper.GetDataMembers(frameType);
                        schema = GenerateSchema(members, frame);
                        createRecordBatch = CreateDataFrameRecordBatchBuilder(frameType, members).Compile();
                    })
                    .IgnoreElements(),
                Observable.Defer(() => CreateDataFrameSink(schema, createRecordBatch, BufferSize).Process(frames))
            ), 1);
        }
    }
}
