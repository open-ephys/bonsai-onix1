using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Controls an RHS2116 stimulus sequence.
    /// </summary>
    /// <remarks>
    /// This operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureHeadstageRhs2116"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    public class Rhs2116StimulusTrigger : Sink<double>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Rhs2116Trigger.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Start an electrical stimulus sequence with an optional hardware delay.
        /// </summary>
        /// <param name="source">A sequence of double values that serves a combined a stimulus trigger and
        /// delay in microseconds. A value of 0 results in immediate stimulus delivery. A value of 100 results
        /// stimulus delivery following a 100 microsecond delay. Delays are implemented in hardware and are
        /// exact. </param>
        /// <returns>A sequence of double values that is identical to <paramref name="source"/></returns>
        public override IObservable<double> Process(IObservable<double> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo =>
                {
                    var device = deviceInfo.GetDeviceContext(typeof(Rhs2116Trigger));
                    return source.Do(t =>
                    {
                        const double SampleFrequencyMegaHz = Rhs2116.SampleFrequencyHz / 1e6;
                        var delaySamples = (int)(t * SampleFrequencyMegaHz);
                        device.WriteRegister(Rhs2116Trigger.TRIGGER, (uint)(delaySamples << 12 | 0x1));
                    });
                });
        }
    }
}
