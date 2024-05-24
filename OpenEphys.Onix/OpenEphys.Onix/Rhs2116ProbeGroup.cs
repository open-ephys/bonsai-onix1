using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Xml.Serialization;
using OpenEphys.ProbeInterface;
using Newtonsoft.Json;
using Bonsai;
using Bonsai.Expressions;

namespace OpenEphys.Onix
{
    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [CombinatorAttribute()]
    [WorkflowElementCategoryAttribute(ElementCategory.Source)]
    public class Rhs2116ProbeGroup : ProbeGroup
    {
        private const int NumChannelsPerProbe = 16;

        public Rhs2116ProbeGroup()
        {
            Specification = "probeinterface";
            Version = "0.2.21";

            Probes = new List<Probe>()
            {
                new(
                    ProbeNdim._2,
                    ProbeSiUnits.Mm,
                    new ProbeAnnotations(),
                    new ContactAnnotations(),
                    DefaultContactPositions(0),
                    DefaultContactPlaneAxes(),
                    DefaultContactShapes(),
                    DefaultContactShapeParams(),
                    DefaultProbePlanarContour(0),
                    DefaultDeviceChannelIndices(0),
                    DefaultContactIds(),
                    DefaultShankIds()),
                new(
                    ProbeNdim._2,
                    ProbeSiUnits.Mm,
                    new ProbeAnnotations(),
                    new ContactAnnotations(),
                    DefaultContactPositions(1),
                    DefaultContactPlaneAxes(),
                    DefaultContactShapes(),
                    DefaultContactShapeParams(),
                    DefaultProbePlanarContour(1),
                    DefaultDeviceChannelIndices(1),
                    DefaultContactIds(),
                    DefaultShankIds()
                    )
            }.ToList();


        }

        [JsonConstructor]
        public Rhs2116ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
        }

        public Rhs2116ProbeGroup(Rhs2116ProbeGroup probeGroup)
            : base(probeGroup)
        {
        }

        private float[][] DefaultContactPositions(int probeIndex)
        {
            float[][] contactPositions = new float[NumChannelsPerProbe][];

            if (probeIndex == 0)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    contactPositions[i] = new float[2] { i + 1.0f, 3.0f };
                }
            }
            else if (probeIndex == 1)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    contactPositions[i] = new float[2] { i + 1.0f, 1.0f };
                }
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default contact positions for {nameof(Rhs2116ProbeGroup)}");
            }

            return contactPositions;
        }

        private float[][][] DefaultContactPlaneAxes()
        {
            float[][][] contactPlaneAxes = new float[NumChannelsPerProbe][][];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactPlaneAxes[i] = new float[2][] { new float[2] { 1.0f, 0.0f }, new float[2] { 0.0f, 1.0f } };
            }

            return contactPlaneAxes;
        }

        private ContactShape[] DefaultContactShapes()
        {
            ContactShape[] contactShapes = new ContactShape[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactShapes[i] = ContactShape.Circle;
            }

            return contactShapes;
        }

        private ContactShapeParam[] DefaultContactShapeParams()
        {
            ContactShapeParam[] contactShapeParams = new ContactShapeParam[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactShapeParams[i] = new ContactShapeParam(0.3f);
            }

            return contactShapeParams;
        }

        private float[][] DefaultProbePlanarContour(int probeIndex)
        {
            float[][] probePlanarContour = new float[5][];

            if (probeIndex == 0)
            {
                probePlanarContour[0] = new float[2] { 0.5f, 2.5f };
                probePlanarContour[1] = new float[2] { 16.5f, 2.5f };
                probePlanarContour[2] = new float[2] { 16.5f, 3.5f };
                probePlanarContour[3] = new float[2] { 0.5f, 3.5f };
                probePlanarContour[4] = new float[2] { 0.5f, 2.5f };
            }
            else if (probeIndex == 1)
            {
                probePlanarContour[0] = new float[2] { 0.5f, 0.5f };
                probePlanarContour[1] = new float[2] { 16.5f, 0.5f };
                probePlanarContour[2] = new float[2] { 16.5f, 1.5f };
                probePlanarContour[3] = new float[2] { 0.5f, 1.5f };
                probePlanarContour[4] = new float[2] { 0.5f, 0.5f };
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default probe planar contours for {nameof(Rhs2116ProbeGroup)}");
            }

            return probePlanarContour;
        }

        private int[] DefaultDeviceChannelIndices(int probeIndex)
        {
            int[] deviceChannelIndices = new int[NumChannelsPerProbe];

            if (probeIndex == 0)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    deviceChannelIndices[i] = i;
                }
            }
            else if (probeIndex == 1)
            {
                for (int i = 0; i < NumChannelsPerProbe; i++)
                {
                    deviceChannelIndices[i] = NumChannelsPerProbe * 2 - 1 - i;
                }
            }
            else
            {
                throw new InvalidOperationException($"Probe {probeIndex} is invalid for getting default device channel indices for {nameof(Rhs2116ProbeGroup)}");
            }

            return deviceChannelIndices;
        }

        private string[] DefaultContactIds()
        {
            string[] contactIds = new string[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactIds[i] = i.ToString();
            }

            return contactIds;
        }

        private string[] DefaultShankIds()
        {
            string[] contactIds = new string[NumChannelsPerProbe];

            for (int i = 0; i < NumChannelsPerProbe; i++)
            {
                contactIds[i] = "";
            }

            return contactIds;
        }

        public IObservable<Rhs2116ProbeGroup> Process()
        {
            return Observable.Defer(() => Observable.Return(new Rhs2116ProbeGroup(this)));
        }

        public IObservable<Rhs2116ProbeGroup> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, _ => new Rhs2116ProbeGroup(this));
        }

        protected virtual bool PrintMembers(StringBuilder stringBuilder)
        {
            stringBuilder.Append("specification = " + Specification + ", ");
            stringBuilder.Append("version = " + Version + ", ");
            stringBuilder.Append("probes = " + Probes);
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" { ");
            if (PrintMembers(stringBuilder) )
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append("}");

            return stringBuilder.ToString();
        }
    }

    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [CombinatorAttribute()]
    [WorkflowElementCategoryAttribute(ElementCategory.Source)]
    public class Probe : ProbeInterface.Probe
    {
        public Probe()
        {
        }

        public Probe(ProbeNdim ndim, ProbeSiUnits si_units, ProbeAnnotations annotations, ContactAnnotations contact_annotations,
            float[][] contact_positions, float[][][] contact_plane_axes, ContactShape[] contact_shapes,
            ContactShapeParam[] contact_shape_params, float[][] probe_planar_contour, int[] device_channel_indices,
            string[] contact_ids, string[] shank_ids)
            : base(ndim, si_units, annotations, contact_annotations, contact_positions, contact_plane_axes, contact_shapes,
                  contact_shape_params, probe_planar_contour, device_channel_indices, contact_ids, shank_ids)
        {
        }

        public Probe(Probe probe)
            : base(probe)
        {
        }

        public IObservable<Probe> Process()
        {
            return Observable.Defer(() => Observable.Return(new Probe(this)));
        }

        public IObservable<Probe> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, _ => new Probe(this));
        }

        protected virtual bool PrintMembers(StringBuilder stringBuilder)
        {
            stringBuilder.Append("ndim = " + NumDimensions + ", ");
            stringBuilder.Append("si_units = " + SiUnits + ", ");
            stringBuilder.Append("annotations = " + Annotations + ", ");
            stringBuilder.Append("contact_annotations = " + ContactAnnotations + ", ");
            stringBuilder.Append("contact_positions = " + ContactPositions + ", ");
            stringBuilder.Append("contact_plane_axes = " + ContactPlaneAxes + ", ");
            stringBuilder.Append("contact_shapes = " + ContactShapes + ", ");
            stringBuilder.Append("contact_shape_params = " + ContactShapeParams + ", ");
            stringBuilder.Append("probe_planar_contour = " + ProbePlanarContour + ", ");
            stringBuilder.Append("device_channel_indices = " + DeviceChannelIndices + ", ");
            stringBuilder.Append("contact_ids = " + ContactIds + ", ");
            stringBuilder.Append("shank_ids = " + ShankIds);
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" { ");
            if (PrintMembers(stringBuilder))
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }

    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [CombinatorAttribute()]
    [WorkflowElementCategoryAttribute(ElementCategory.Source)]
    public class ProbeAnnotations : ProbeInterface.ProbeAnnotations
    {
        public ProbeAnnotations()
            : base()
        {
        }

        public ProbeAnnotations(string name, string manufacturer)
            : base(name, manufacturer)
        {
        }

        protected ProbeAnnotations(ProbeAnnotations probe)
            : base(probe)
        {
        }

        public IObservable<ProbeAnnotations> Process()
        {
            return Observable.Defer(() => Observable.Return(new ProbeAnnotations(this)));
        }

        public IObservable<ProbeAnnotations> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, _ => new ProbeAnnotations(this));
        }

        protected virtual bool PrintMembers(StringBuilder stringBuilder)
        {
            stringBuilder.Append("name = " + Name + ", ");
            stringBuilder.Append("manufacturer = " + Manufacturer);
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" { ");
            if (PrintMembers(stringBuilder))
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }

    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [CombinatorAttribute()]
    [WorkflowElementCategoryAttribute(ElementCategory.Source)]
    public class ContactShapeParam : ProbeInterface.ContactShapeParam
    {
        public ContactShapeParam()
            : base() 
        {
        }

        public ContactShapeParam(float radius)
            : base(radius)
        {
        }

        protected ContactShapeParam(ContactShapeParam contactShapeParam)
            : base(contactShapeParam)
        {
        }

        public IObservable<ContactShapeParam> Process()
        {
            return Observable.Defer(() => Observable.Return(new ContactShapeParam(this)));
        }

        public IObservable<ContactShapeParam> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, _ => new ContactShapeParam(this));
        }

        protected virtual bool PrintMembers(StringBuilder stringBuilder)
        {
            stringBuilder.Append("radius = " + Radius + ", ");
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" { ");
            if (PrintMembers(stringBuilder))
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Serializes a sequence of data model objects into JSON strings.
    /// </summary>
    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [DescriptionAttribute("Serializes a sequence of data model objects into JSON strings.")]
    [CombinatorAttribute()]
    [WorkflowElementCategoryAttribute(ElementCategory.Transform)]
    public partial class SerializeToJson
    {
        private IObservable<string> Process<T>(IObservable<T> source)
        {
            return Observable.Select(source, value => JsonConvert.SerializeObject(value));
        }

        public IObservable<string> Process(IObservable<Rhs2116ProbeGroup> source)
        {
            return Process<Rhs2116ProbeGroup>(source);
        }

        public IObservable<string> Process(IObservable<Probe> source)
        {
            return Process<Probe>(source);
        }

        public IObservable<string> Process(IObservable<ProbeAnnotations> source)
        {
            return Process<ProbeAnnotations>(source);
        }

        public IObservable<string> Process(IObservable<ContactShapeParam> source)
        {
            return Process<ContactShapeParam>(source);
        }
    }

    /// <summary>
    /// Deserializes a sequence of JSON strings into data model objects.
    /// </summary>
    [GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
    [DescriptionAttribute("Deserializes a sequence of JSON strings into data model objects.")]
    [DefaultPropertyAttribute("Type")]
    [WorkflowElementCategoryAttribute(ElementCategory.Transform)]
    [XmlIncludeAttribute(typeof(TypeMapping<Rhs2116ProbeGroup>))]
    [XmlIncludeAttribute(typeof(TypeMapping<Probe>))]
    [XmlIncludeAttribute(typeof(TypeMapping<ProbeAnnotations>))]
    [XmlIncludeAttribute(typeof(TypeMapping<ContactShapeParam>))]
    public partial class DeserializeFromJson : SingleArgumentExpressionBuilder
    {
        public DeserializeFromJson()
        {
            Type = new TypeMapping<Rhs2116ProbeGroup>();
        }

        public TypeMapping Type { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var typeMapping = (TypeMapping)Type;
            var returnType = typeMapping.GetType().GetGenericArguments()[0];
            return Expression.Call(
                typeof(DeserializeFromJson),
                "Process",
                new Type[] { returnType },
                Enumerable.Single(arguments));
        }

        private static IObservable<T> Process<T>(IObservable<string> source)
        {
            return Observable.Select(source, value => JsonConvert.DeserializeObject<T>(value));
        }
    }
}
