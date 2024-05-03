using System.Collections.Generic;
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

        public int NumContacts
        {
            get
            {
                int numContacts = 0;

                foreach (var probe in Probes)
                {
                    numContacts += probe.NumContacts;
                }

                return numContacts;
            }
        }

        public string[] GetContactIds()
        {
            string[] contactIds = new string[NumContacts];

            var length = 0;

            foreach (var probe in Probes)
            {
                probe.Contact_Ids.CopyTo(contactIds, length);
                length += probe.NumContacts;
            }

            return contactIds;
        }
    }

    public class Probe
    {
        public uint Ndim { get; set; }
        public string Si_Units { get; set; }
        public float[][] Contact_Positions { get; set; }
        public float[][][] Contact_Plane_Axes { get; set; }
        public string[] Contact_Shapes { get; set; }
        public ContactShapeParam[] Contact_Shape_Params { get; set; }
        public float[][] Probe_Planar_Contour { get; set; }
        public uint[] Device_Channel_Indices { get; set; }
        public string[] Contact_Ids { get; set; }
        public string[] Shank_Ids { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public Probe(uint ndim, string si_units, float[][] contact_positions, float[][][] contact_plane_axes, string[] contact_shapes, 
            ContactShapeParam[] contact_shape_params, float[][] probe_planar_contour, uint[] device_channel_indices, string[] contact_ids, string[] shank_Ids)
        {
            Ndim = ndim;
            Si_Units = si_units;
            Contact_Positions = contact_positions;
            Contact_Plane_Axes = contact_plane_axes;
            Contact_Shapes = contact_shapes;
            Contact_Shape_Params = contact_shape_params;
            Probe_Planar_Contour = probe_planar_contour;
            Device_Channel_Indices = device_channel_indices;
            Contact_Ids = contact_ids;
            Shank_Ids = shank_Ids;
        }

        /// <summary>
        /// Returns a Contact object that contains the position, shape, shape params, and IDs (device / contact / shank)
        /// </summary>
        /// <param name="index">Relative index of the contact in this Probe</param>
        /// <returns></returns>
        public Contact GetContact(int index)
        {
            return new Contact(Contact_Positions[index][0], Contact_Positions[index][1], Contact_Shapes[index], Contact_Shape_Params[index],
                Device_Channel_Indices[index], Contact_Ids[index], Shank_Ids[index]);
        }

        public int NumContacts => Contact_Ids.Length;
    }
    public struct Contact
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public string Shape { get; set; }
        public ContactShapeParam ShapeParams { get; set; }
        public uint DeviceId { get; set; }
        public string ContactId { get; set; }
        public string ShankId { get; set; }

        public Contact(float posX,  float posY, string shape, ContactShapeParam shapeParam, uint device_id,  string contact_id, string shank_id)
        {
            PosX = posX;
            PosY = posY;
            Shape = shape;
            ShapeParams = shapeParam;
            DeviceId = device_id;
            ContactId = contact_id;
            ShankId = shank_id;
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
