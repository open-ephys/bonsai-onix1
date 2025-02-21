using Bonsai;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Creates a <see cref="ContextTask"/> that orchestrates data acquisition for an ONIX system.
    /// </summary>
    /// <remarks>
    /// ONIX is built on top of the <see href="https://open-ephys.github.io/ONI/">Open Neuro Interface
    /// (ONI)</see> hardware specification and API. One of ONI's requirements is the creation of a "context"
    /// that holds information needed for communication between a host computer and ONI-compliant hardware.
    /// The context holds data such as the device driver that will be used to communicate with hardware, what
    /// devices (e.g. headstages and their internal components) are currently connected, how
    /// often data should be read by the host computer, etc. <see cref="CreateContext"/> creates this required
    /// ONI context for a single ONIX system. The ONIX system that the context serves is uniquely identified
    /// within a host computer by the <see cref="Driver"/> used to communicate with hardware and the <see
    /// cref="Index"/>, which is a enumeration that is translated by the driver into a physical interface
    /// (e.g. a particular PCIe slot) within the host computer.
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
        [TypeConverter(typeof(ContextDriverConverter))]
        public string Driver { get; set; } = ContextTask.DefaultDriver;

        /// <summary>
        /// Gets or sets the index of the host interconnect between the ONI controller and host computer.
        /// </summary>
        /// <remarks>
        /// For instance, 0 could correspond to a particular PCIe slot or USB port as enumerated by the
        /// operating system and translated by an <see
        /// href="https://open-ephys.github.io/ONI/">ONI
        /// device driver translator</see>. A value of -1 will attempt to open the default index and is useful
        /// if there is only a single ONI controller managed by the specified selected <see cref="Driver"/> in
        /// the host computer.
        /// </remarks>
        [Description("The index of the host interconnect between the ONI controller and host computer.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int Index { get; set; } = ContextTask.DefaultIndex;

        /// <summary>
        /// Generates a sequence that creates a new <see cref="ContextTask"/> object.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="ContextTask"/> class. Cancelling the
        /// sequence will dispose of the created context.
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
