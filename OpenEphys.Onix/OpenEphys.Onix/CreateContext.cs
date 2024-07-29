using Bonsai;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Produces a sequence of <see cref="ContextTask"/> elements, each of which orchestrate a single ONI-compliant controller.
    /// </summary>
    [Description("Produces a sequence of ContextTasks, each of which orchestrate a single ONI-compliant controller.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class CreateContext
    {
        /// <summary>
        /// Gets or sets a string specifying the device driver used to communicate with hardware.
        /// </summary>
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
        public int Index { get; set; } = ContextTask.DefaultIndex;

        /// <summary>
        /// Produces a sequence of <see cref="ContextTask"/> elements that orchestrate ONI compliant controller.
        /// </summary>
        /// <returns>A sequence of <see cref="ContextTask"/> elements that orchestrate ONI compliant controller.</returns>
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
