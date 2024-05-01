using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenEphys.Onix.Design
{
    public class ProbeGroup
    {
        public string Specification { get; set; }
        public string Version { get; set; }
        public Probe[] Probes { get; set; }

        public ProbeGroup(string specification, string version, Probe[] probes)
        {
            Specification = specification;
            Version = version;
            Probes = probes;
        }
    }

    public class Probe
    {
        public uint Ndim { get; set; }
        public string Si_Units { get; set; }
        public float[][] Contact_Positions { get; set; }
        public string[] Contact_Shapes { get; set; }
        public ContactShapeParam[] Contact_Shape_Params { get; set; }
        public float[][] Probe_Planar_Contours { get; set; }
        public uint[] Device_Channel_Indices { get; set; }
        public uint[] Contact_Ids { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public Probe(uint ndim, string si_units, float[][] contact_positions, string[] contact_shapes, 
            ContactShapeParam[] contact_shape_params, float[][] probe_planar_contours, uint[] device_channel_indices, uint[] contact_ids)
        {
            Ndim = ndim;
            Si_Units = si_units;
            Contact_Positions = contact_positions;
            Contact_Shapes = contact_shapes;
            Contact_Shape_Params = contact_shape_params;
            Probe_Planar_Contours = probe_planar_contours;
            Device_Channel_Indices = device_channel_indices;
            Contact_Ids = contact_ids;
        }
    }
    public struct ContactShapeParam
    {
        public float Radius { get; set; }

        public ContactShapeParam(float radius) 
        { 
            Radius = radius;
        }
    }
}
