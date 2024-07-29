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
                UpdateDeviceNames();
            }
        }

        internal string GetFullDeviceName(string deviceName)
        {
            return !string.IsNullOrEmpty(_name) ? $"{_name}/{deviceName}" : string.Empty;
        }

        internal virtual void UpdateDeviceNames()
        {
            foreach (var device in GetDevices())
            {
                device.DeviceName = GetFullDeviceName(device.DeviceType.Name);
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
