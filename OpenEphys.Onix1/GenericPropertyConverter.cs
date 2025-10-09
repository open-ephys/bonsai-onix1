using System;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Generic property converter that expands to show all properties.
    /// </summary>
    internal class GenericPropertyConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = (from property in base.GetProperties(context, value, attributes).Cast<PropertyDescriptor>()
                              where !property.IsReadOnly
                              select property)
                              .ToArray();
            return new PropertyDescriptorCollection(properties).Sort(properties.Select(p => p.Name).ToArray());
        }
    }
}
