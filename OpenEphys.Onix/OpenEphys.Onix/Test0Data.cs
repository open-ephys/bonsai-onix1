using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Test0Data : Source<Test0DataFrame>
    {
        [TypeConverter(typeof(Test0.NameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<Test0DataFrame> Generate()
        {

            return Observable.Using(
                () => DeviceManager.ReserveDevice(DeviceName),
                disposable => disposable.Subject.SelectMany(deviceInfo =>
                {
                    var device = deviceInfo.GetDevice(typeof(Test0));

                    // Find payload size
                    var dummyWords = deviceInfo.Context.ReadRegister(deviceInfo.DeviceAddress, Test0.NUMTESTWORDS);

                    return deviceInfo.Context.FrameReceived
                        .Where(frame => frame.DeviceAddress == device.Address)
                        .Select(frame => new Test0DataFrame(frame, (int)dummyWords));
                }));
        }
    }
}
