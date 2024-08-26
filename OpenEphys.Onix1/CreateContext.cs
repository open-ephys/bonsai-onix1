using Bonsai;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Creates a <see cref="ContextTask"/> to that orchestrates data acquisition for single ONIX.
    /// </summary>
    /// <remarks>
    /// ONIX is built on top of the <see href="https://open-ephys.github.io/ONI/">ONI</see> hardware specification and API.
    /// One of ONI's requirements is the creation of a context that holds information that used for communication between the
    /// computer and ONI-compliant hardware. The context holds data such as the device driver that will be used to communicate
    /// with hardware, what devices (e.g. headstages) are currently attached to the hardware, how often data should be read
    /// from hardware, the run state of the system, etc. <see cref="ContextTask"/> creates a ONI context for a single ONIX
    /// system. The system is uniquely identified with a host computer by the <see cref="Driver"/> used to communicate with
    /// hardware and the <see cref="Index"/>, which is a enumeration that is translated by the driver into a physical location
    /// (e.g. a particular PCIe slot) that the hardware uses to communicate with the host computer.
    /// </remarks>
    [Description("Creates a ContextTask to orchestrate a single ONI-compliant controller using the specified device driver and host interconnect.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class CreateContext
    {
        /// <summary>
        /// Gets or sets a string specifying the device driver used to communicate with hardware.
        /// </summary>
        [Description("Specifies the device driver used to communicate with hardware.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string Driver { get; set; } = ContextTask.DefaultDriver;

        /// <summary>
        /// Gets or sets the index of the host interconnect between the ONI controller and host computer.
        /// </summary>
        /// <remarks>
        /// For instance, 0 could correspond to a particular PCIe slot or USB port as enumerated by the operating system and translated by an
        /// <see href="https://open-ephys.github.io/ONI/api/liboni/driver-translators/index.html#drivers">ONI device driver translator</see>. 
        /// A value of -1 will attempt to open the default index and is useful if there is only a single ONI controller
        /// managed by the specified selected <see cref="Driver"/> in the host computer.
        /// </remarks>
        [Description("The index of the host interconnect between the ONI controller and host computer.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int Index { get; set; } = ContextTask.DefaultIndex;

        /// <summary>
        /// Generates a sequence that creates a new <see cref="ContextTask"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="ContextTask"/> class. Cancelling the sequence
        /// will dispose of the created context.
        /// </returns>
        public IObservable<ContextTask> Generate()
        {
            return Observable.Create<ContextTask>(observer =>
            {
                var driver = Driver;
                var index = Index;
                var context = new ContextTask(driver, index);
                try
                {
                    observer.OnNext(context);
                    return context;
                }
                catch
                {
                    context.Dispose();
                    throw;
                }
            });
        }
    }
}
