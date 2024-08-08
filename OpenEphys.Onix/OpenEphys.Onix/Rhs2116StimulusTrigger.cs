using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Rhs2116StimulusTrigger : Sink<double>
    {
        [TypeConverter(typeof(Rhs2116Trigger.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<double> Process(IObservable<double> source)
        {
            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                    return source.Do(t =>
                    {
                        const double SampleFrequencyMegaHz = Rhs2116.SampleFrequencyHz / 1e6;
                        var delaySamples = (int)(t * SampleFrequencyMegaHz);
                        device.WriteRegister(Rhs2116Trigger.TRIGGER, (uint)(delaySamples << 12 | 0x1));
                    });

                }));
        }
    }
}
