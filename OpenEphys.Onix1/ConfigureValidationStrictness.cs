using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Sets the hardware validation strictness of a <see cref="ContextTask"/>.
    /// </summary>
    /// <remarks>
    /// Downstream configuration operators may validate the hardware they configure before allowing
    /// acquisition to proceed. This operator controls how failures of those checks are treated, making it
    /// possible to relax them for debugging or rapid prototyping.
    /// <para>
    /// Relaxing validation strictness does not change what data is acquired, only whether unmet preconditions
    /// are treated as fatal. Data acquired under a relaxed strictness therefore cannot be assumed to be
    /// correct.
    /// </para>
    /// <para>
    /// Which checks exist, and which strictness levels relax them, varies by configuration operator. See the
    /// documentation of the specific operator or property being configured for details.
    /// </para>
    /// </remarks>
    [Description("Sets the hardware validation strictness of a ContextTask.")]
    public class ConfigureValidationStrictness : Transform<ContextTask, ContextTask>
    {
        /// <summary>
        /// Gets or sets the hardware validation strictness.
        /// </summary>
        [Description("Specifies how strictly hardware validation checks are enforced.")]
        public ValidationStrictness Strictness { get; set; } = ValidationStrictness.Normal;

        /// <summary>
        /// Sets the hardware validation strictness of a <see cref="ContextTask"/>.
        /// </summary>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence with the <see cref="ContextTask.Strictness"/> property updated.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            return source.ConfigureAndLatchController(context =>
            {
                context.Strictness = Strictness;
                return Disposable.Empty;
            });
        }
    }
}
