using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Hexa.NET.ImGui;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The signal a probe scope is being asked to display: which band, whether it is referenced against the
    /// median of the channels sharing its converter, and whether each channel's offset is removed.
    /// </summary>
    internal readonly record struct BandSelection(int Band, bool CommonMedianReference, bool AcCoupled);

    /// <summary>
    /// The columns a <see cref="ProbeScopeVisualizer{TFrame}"/> adds to the control strip, for the band the probe
    /// is being viewed in and the filters applied to it, and the selection they make.
    /// </summary>
    /// <remarks>
    /// Referencing and AC coupling sit here beside the band because each is a filter: referencing a spatial
    /// high-pass across the ADC group, AC coupling a temporal one on each channel. Turning either on defines a
    /// different signal exactly as choosing another band does, and all three rebind the chain.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type of the scope whose settings the columns show.</typeparam>
    internal sealed class ImGuiProbeScopeBandColumns<TFrame>
    {
        readonly ProbeScope<TFrame> scope;
        readonly Func<bool> paused;
        readonly BehaviorSubject<BandSelection> selection;

        /// <param name="scope">The node whose band, referencing and AC coupling the columns show and set.</param>
        /// <param name="paused">Whether the display is paused, while which the columns are disabled.</param>
        public ImGuiProbeScopeBandColumns(ProbeScope<TFrame> scope, Func<bool> paused)
        {
            this.scope = scope;
            this.paused = paused;
            selection = new(Current);
            BandColumn = new("Band", DrawBand);
            FiltersColumn = new("Filters", DrawFilters);
        }

        /// <summary>
        /// The column choosing the band shown.
        /// </summary>
        public ControlStripColumn BandColumn { get; }

        /// <summary>
        /// The column turning referencing and AC coupling on and off.
        /// </summary>
        public ControlStripColumn FiltersColumn { get; }

        /// <summary>
        /// The bands to offer, in display order. The owner sets this once the probe is known.
        /// </summary>
        public IReadOnlyList<ProbeScopeBand<TFrame>> Bands { get; set; } = Array.Empty<ProbeScopeBand<TFrame>>();

        /// <summary>
        /// The signal being asked for, starting with whatever the strip opens on.
        /// </summary>
        public IObservable<BandSelection> BandSelected => selection;

        /// <summary>
        /// Whether the probe offers referencing. The owner sets this once the probe is known.
        /// </summary>
        public bool ReferenceOffered { get; set; } = true;

        /// <summary>
        /// The scope's band by name, or the first band where the bands are not yet known or do not include it.
        /// </summary>
        int Band
        {
            get
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    if (Bands[i].Name == scope.Band)
                        return i;
                }

                return 0;
            }
        }

        BandSelection Current => new(Band, scope.CommonMedianReference, scope.AcCoupled);

        /// <summary>
        /// Asks for the signal the scope's settings describe if it is not the one last asked for, as when they are
        /// changed in the property grid or once the bands are known. Called once a frame.
        /// </summary>
        public void RequestSelection()
        {
            var current = Current;
            if (current != selection.Value)
                selection.OnNext(current);
        }

        // NB: a change while paused would not show until resumed.
        void DrawBand(float width)
        {
            ImGui.BeginDisabled(paused());
            ImGui.SetNextItemWidth(width);
            BandCombo();
            ImGui.EndDisabled();
        }

        void DrawFilters(float width)
        {
            ImGui.BeginDisabled(paused());

            // NB: shown unchecked where the probe does not offer it, as AC is where the band does not.
            ImGui.BeginDisabled(!ReferenceOffered);
            var cmr = scope.CommonMedianReference && ReferenceOffered;
            if (ImGui.Checkbox("CMR", ref cmr))
                scope.CommonMedianReference = cmr;
            ImGuiWidgets.Tooltip("Common median reference: subtract from each channel, at each moment, the median of " +
                "the channels that share its ADC, which removes noise they have in common.");
            ImGui.EndDisabled();

            ImGui.SameLine();
            var band = Band;
            var acOffered = band >= Bands.Count || Bands[band].AcDescription is not null;
            ImGui.BeginDisabled(!acOffered);

            // NB: shown unchecked where the band does not offer it, since nothing is applied there, while the
            // choice is kept for the next band that does.
            var ac = scope.AcCoupled && acOffered;
            if (ImGui.Checkbox("AC", ref ac))
                scope.AcCoupled = ac;
            ImGuiWidgets.Tooltip("Show only the AC part of each channel's signal, using a 1 Hz high-pass.",
                "This band is AC: it is high-pass filtered.");
            ImGui.EndDisabled();

            ImGui.EndDisabled();
        }

        void BandCombo()
        {
            var band = Band;
            var singleBand = Bands.Count <= 1;
            var preview = band >= 0 && band < Bands.Count ? Bands[band].Name : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            var open = ImGui.BeginCombo("##band", preview);
            ImGuiWidgets.Tooltip("Signal shown: wideband, the spike band or the LFP band, as the probe offers them.",
                "This probe offers one band.");
            if (open)
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    var isSelected = i == band;
                    var passband = scope.AcCoupled && Bands[i].AcDescription is not null
                        ? Bands[i].AcDescription
                        : Bands[i].Description;
                    if (ImGui.Selectable($"{Bands[i].Name}: {passband}", isSelected) && !isSelected)
                        scope.Band = Bands[i].Name;
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (singleBand) ImGui.EndDisabled();
        }
    }
}
