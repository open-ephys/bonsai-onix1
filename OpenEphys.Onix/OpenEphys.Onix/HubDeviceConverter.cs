using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace OpenEphys.Onix
{
    internal class HubDeviceConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var device = (SingleDeviceFactory)value;
                return $"Index: {device.DeviceIndex}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = (from property in base.GetProperties(context, value, attributes).Cast<PropertyDescriptor>()
                              where !property.IsReadOnly && property.ComponentType != typeof(SingleDeviceFactory)
                              select property)
                              .ToArray();
            return new PropertyDescriptorCollection(properties).Sort(properties.Select(p => p.Name).ToArray());
        }
    }
}
