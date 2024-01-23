using System;
using Bonsai;

namespace OpenEphys.Onix
{
    public abstract class HubDeviceFactory : DeviceFactory, INamedElement
    {
        const string BaseTypePrefix = "Configure";
        string _name;

        protected HubDeviceFactory()
        {
            var baseName = GetType().Name;
            var prefixIndex = baseName.IndexOf(BaseTypePrefix);
            Name = prefixIndex >= 0 ? baseName.Substring(prefixIndex + BaseTypePrefix.Length) : baseName;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                UpdateDeviceNames(_name);
            }
        }

        internal virtual void UpdateDeviceNames(string hubName)
        {
            foreach (var device in GetDevices())
            {
                device.DeviceName = !string.IsNullOrEmpty(hubName)
                    ? $"{hubName}.{device.DeviceType.Name}"
                    : string.Empty;
            }
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            if (string.IsNullOrEmpty(_name))
            {
                throw new InvalidOperationException("A valid hub device name must be specified.");
            }

            var output = source;
            foreach (var device in GetDevices())
            {
                output = device.Process(output);
            }

            return output;
        }
    }
}
