using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace OpenEphys.Onix1
{
    class DeviceManager
    {
        static readonly Dictionary<string, DeviceDisposable> deviceMap = new();
        static readonly object managerLock = new();

        internal static IDisposable RegisterDevice(string name, DeviceContext device, Type deviceType)
        {
            var deviceInfo = new DeviceInfo(device, deviceType);
            return RegisterDevice(name, deviceInfo);
        }

        internal static IDisposable RegisterDevice(string name, DeviceInfo deviceInfo)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A valid device name must be specified.", nameof(name));
            }

            lock (managerLock)
            {
                var disposable = RegisterDevice(name);
                var subject = disposable.Subject;

                foreach (var entry in deviceMap)
                {
                    if (!entry.Value.Subject.IsCompleted)
                    {
                        continue;
                    }

                    var info = entry.Value.Subject.GetResult();
                    if (info.Context == deviceInfo.Context && info.DeviceAddress == deviceInfo.DeviceAddress)
                    {
                        throw new ArgumentException(
                            $"The specified device '{deviceInfo.DeviceType.Name}' could not be registered " +
                            $"because another device '{info.DeviceType.Name}' with the same address " +
                            $"{info.DeviceAddress} has already been configured in this context.",
                            nameof(deviceInfo)
                        );
                    }
                }

                subject.OnNext(deviceInfo);
                subject.OnCompleted();
                return disposable;
            }
        }

        static DeviceDisposable RegisterDevice(string name)
        {
            lock (managerLock)
            {
                if (deviceMap.ContainsKey(name))
                {
                    throw new ArgumentException(
                        $"A device with the same name '{name}' has already been configured.",
                        nameof(name)
                    );
                }

                var subject = new AsyncSubject<DeviceInfo>();
                var dispose = Disposable.Create(() =>
                {
                    subject.Dispose();
                    deviceMap.Remove(name);
                });

                var deviceDisposable = new DeviceDisposable(subject, dispose);
                deviceMap.Add(name, deviceDisposable);
                return deviceDisposable;
            }
        }

        internal static IObservable<DeviceInfo> GetDevice(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A valid device name must be specified.", nameof(name));
            }

            return Observable.Create<DeviceInfo>(observer =>
            {
                lock (managerLock)
                {
                    if (!deviceMap.TryGetValue(name, out var deviceDisposable))
                    {
                        throw new ArgumentException(
                            $"No device with the specified name '{name}' has been configured.",
                            nameof(name)
                        );
                    }

                    return deviceDisposable.Subject.SubscribeSafe(observer);
                }
            });
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
