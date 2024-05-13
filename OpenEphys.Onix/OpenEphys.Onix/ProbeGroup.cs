using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    public class ProbeGroup
    {
        public string Specification { get; set; }
        public string Version { get; set; }
        public Probe[] Probes { get; set; }

        public ProbeGroup()
        {
            Specification = string.Empty;
            Version = string.Empty;
            Probes = new Probe[0];
        }

        public ProbeGroup(string specification, string version, Probe[] probes)
        {
            Specification = specification;
            Version = version;
            Probes = probes;

            ValidateContactIds();
            ValidateShankIds();
            ValidateDeviceChannelIndices();
        }

        public ProbeGroup(ProbeGroup probeGroup)
        {
            Specification = probeGroup.Specification;
            Version = probeGroup.Version;
            Probes = probeGroup.Probes;
        }

        public bool IsValid => Specification != string.Empty && Version != string.Empty && Probes.Length > 0;

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

        private void ValidateContactIds()
        {
            int contactNum = 0;

            for (int i = 0; i < Probes.Length; i++)
            {
                if (Probes[i].Contact_Ids == null)
                {
                    Probes[i].Contact_Ids = new string[Probes[i].Contact_Positions.Length];

                    for (int j = 0; j < Probes[i].Contact_Ids.Length; j++)
                    {
                        Probes[i].Contact_Ids[j] = contactNum.ToString();
                        contactNum++;
                    }
                }
                else
                    contactNum += Probes[i].Contact_Ids.Length;
            }
        }

        private void ValidateShankIds()
        {
            for (int i = 0; i < Probes.Length; i++)
            {
                if (Probes[i].Shank_Ids == null)
                {
                    Probes[i].Shank_Ids = new string[Probes[i].Contact_Positions.Length];
                }
            }
        }

        private void ValidateDeviceChannelIndices()
        {
            for (int i = 0; i < Probes.Length; i++)
            {
                if (Probes[i].Device_Channel_Indices == null)
                {
                    Probes[i].Device_Channel_Indices = new uint[Probes[i].Contact_Ids.Length];

                    for (int j = 0; j < Probes[i].Device_Channel_Indices.Length; j++)
                    {
                        if (uint.TryParse(Probes[i].Contact_Ids[j], out uint result))
                        {
                            Probes[i].Device_Channel_Indices[j] = result;
                        }
                    }
                }
            }
        }
    }

    public class Probe
    {
        public uint Ndim { get; set; }
        public string Si_Units { get; set; }
        public ProbeAnnotations Annotations { get; set; }
        public ContactAnnotations Contact_Annotations { get; set; }
        public float[][] Contact_Positions { get; set; }
        public float[][][] Contact_Plane_Axes { get; set; }
        public string[] Contact_Shapes { get; set; }
        public ContactShapeParam[] Contact_Shape_Params { get; set; }
        public float[][] Probe_Planar_Contour { get; set; }
        public uint[] Device_Channel_Indices { get; set; }
        public string[] Contact_Ids { get; set; }
        public string[] Shank_Ids { get; set; }

        public Probe()
        {
            Ndim = 0;
            Si_Units = string.Empty;
            Annotations = new ProbeAnnotations();
            Contact_Annotations = new ContactAnnotations();
            Contact_Positions = new float[0][];
            Contact_Plane_Axes = new float[0][][];
            Contact_Shapes = new string[0];
            Contact_Shape_Params = new ContactShapeParam[0];
            Probe_Planar_Contour = new float[0][];
            Device_Channel_Indices = new uint[0];
            Contact_Ids = new string[0];
            Shank_Ids = new string[0];
        }

        public Probe(uint ndim, string si_units, ProbeAnnotations annotations, ContactAnnotations contact_annotations, 
            float[][] contact_positions, float[][][] contact_plane_axes, string[] contact_shapes, 
            ContactShapeParam[] contact_shape_params, float[][] probe_planar_contour, uint[] device_channel_indices, 
            string[] contact_ids, string[] shank_Ids)
        {
            Ndim = ndim;
            Si_Units = si_units;
            Annotations = annotations;
            Contact_Annotations = contact_annotations;
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
        /// for a single contact at the given index
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

        public Contact(float posX, float posY, string shape, ContactShapeParam shapeParam, uint device_id, string contact_id, string shank_id)
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

        public ContactShapeParam()
        {
            Radius = 0;
        }

        public ContactShapeParam(float radius)
        {
            Radius = radius;
        }
    }

    public struct ProbeAnnotations
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }

        public ProbeAnnotations(string name, string manufacturer)
        {
            Name = name;
            Manufacturer = manufacturer;
        }

        public ProbeAnnotations()
        {
            Name = "";
            Manufacturer = "";
        }
    }

    public struct ContactAnnotations
    {
        public string[] Annotations { get; set; }

        public ContactAnnotations()
        {
            Annotations = new string[0];
        }

        public ContactAnnotations(string[] contact_annotations)
        {
            Annotations = contact_annotations;
        }
    }
}
