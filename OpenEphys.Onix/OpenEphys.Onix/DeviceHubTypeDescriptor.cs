using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix
{
    class DeviceHubTypeDescriptor : CustomTypeDescriptor
    {
        readonly DeviceFactory hub;

        public DeviceHubTypeDescriptor(DeviceFactory instance)
        {
            hub = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(Array.Empty<Attribute>());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = from entry in hub.GetDevices().Select((device, index) => (device, index))
                             let category = new CategoryAttribute(entry.device.Type.Name)
                             from property in TypeDescriptor.GetProperties(entry.device, attributes).Cast<PropertyDescriptor>()
                             where !property.IsReadOnly && property.ComponentType != typeof(SingleDeviceFactory)
                             select new IndexedPropertyDescriptor(property, entry.index, category);
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            if (pd is IndexedPropertyDescriptor indexedProperty &&
                hub.GetDevices() is IList<IDeviceConfiguration> devices)
            {
                return devices[indexedProperty.Index];
            }

            return base.GetPropertyOwner(pd);
        }

        class IndexedPropertyDescriptor : PropertyDescriptor
        {
            readonly PropertyDescriptor descriptor;

            public IndexedPropertyDescriptor(PropertyDescriptor descr, int index, params Attribute[] attrs)
                : base(descr, attrs)
            {
                Index = index;
                descriptor = descr;
            }

            internal int Index { get; }

            public override Type ComponentType => descriptor.ComponentType;

            public override bool IsReadOnly => descriptor.IsReadOnly;

            public override Type PropertyType => descriptor.PropertyType;

            private object GetPropertyOwner(object component)
            {
                var hub = (DeviceFactory)component;
                var devices = hub.GetDevices();
                if (devices is IList<IDeviceConfiguration> deviceList)
                {
                    return deviceList[Index];
                }

                return devices.ElementAt(Index);
            }

            public override bool CanResetValue(object component)
            {
                var owner = GetPropertyOwner(component);
                return descriptor.CanResetValue(owner);
            }

            public override object GetValue(object component)
            {
                var owner = GetPropertyOwner(component);
                return descriptor.GetValue(owner);
            }

            public override void ResetValue(object component)
            {
                var owner = GetPropertyOwner(component);
                descriptor.ResetValue(owner);
            }

            public override void SetValue(object component, object value)
            {
                var owner = GetPropertyOwner(component);
                descriptor.SetValue(owner, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                var owner = GetPropertyOwner(component);
                return descriptor.ShouldSerializeValue(owner);
            }
        }
    }
}
