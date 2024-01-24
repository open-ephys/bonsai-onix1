using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace OpenEphys.Onix
{
    class DeviceManager
    {
        static readonly Dictionary<string, ResourceHandle> deviceMap = new();
        static readonly object managerLock = new();

        internal static IDisposable RegisterDevice(string name, DeviceContext device, Type deviceType)
        {
            var deviceInfo = new DeviceInfo(device.Context, deviceType, device.Address);
            return RegisterDevice(name, deviceInfo);
        }

        internal static IDisposable RegisterDevice(string name, DeviceInfo deviceInfo)
        {
            lock (managerLock)
            {
                var disposable = ReserveDevice(name);
                var subject = disposable.Subject;
                if (subject.IsCompleted)
                {
                    throw new InvalidOperationException(
                        "A device with the same name has already been configured."
                    );
                }

                foreach (var entry in deviceMap)
                {
                    if (!entry.Value.Subject.IsCompleted)
                    {
                        continue;
                    }

                    var info = entry.Value.Subject.GetResult();
                    if (info.Context == deviceInfo.Context && info.DeviceAddress == deviceInfo.DeviceAddress)
                    {
                        throw new InvalidOperationException(
                            "A device with the same address has already been configured in this context."
                        );
                    }
                }

                subject.OnNext(deviceInfo);
                subject.OnCompleted();
                return disposable;
            }
        }

        internal static DeviceDisposable ReserveDevice(string name)
        {
            lock (managerLock)
            {
                if (!deviceMap.TryGetValue(name, out var resourceHandle))
                {
                    var subject = new AsyncSubject<DeviceInfo>();
                    var dispose = Disposable.Create(() =>
                    {
                        subject.Dispose();
                        deviceMap.Remove(name);
                    });

                    resourceHandle.Subject = subject;
                    resourceHandle.RefCount = new RefCountDisposable(dispose);
                    deviceMap.Add(name, resourceHandle);
                    return new DeviceDisposable(subject, resourceHandle.RefCount);
                }

                return new DeviceDisposable(
                    resourceHandle.Subject,
                    resourceHandle.RefCount.GetDisposable());
            }
        }

        struct ResourceHandle
        {
            public AsyncSubject<DeviceInfo> Subject;
            public RefCountDisposable RefCount;
        }

        internal sealed class DeviceDisposable : IDisposable
        {
            IDisposable resource;

            public DeviceDisposable(AsyncSubject<DeviceInfo> subject, IDisposable disposable)
            {
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
            }

            public AsyncSubject<DeviceInfo> Subject { get; private set; }

            public void Dispose()
            {
                lock (managerLock)
                {
                    if (resource != null)
                    {
                        resource.Dispose();
                        resource = null;
                    }
                }
            }
        }
    }
}
