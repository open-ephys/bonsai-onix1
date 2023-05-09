using System.ComponentModel;

namespace OpenEphys.Onix
{
    class DeviceHubTypeDescriptionProvider<THub> : TypeDescriptionProvider where THub : DeviceFactory
    {
        static readonly TypeDescriptionProvider BaseProvider = TypeDescriptor.GetProvider(typeof(THub));

        public DeviceHubTypeDescriptionProvider()
            : base(BaseProvider)
        {
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            var hub = (DeviceFactory)instance;
            if (hub != null)
            {
                return new DeviceHubTypeDescriptor(hub);
            }

            return base.GetExtendedTypeDescriptor(instance);
        }
    }
}
