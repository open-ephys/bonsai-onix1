using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The signal a probe scope is being asked to display: which band, whether it is referenced against the
    /// median of the channels sharing its converter, and whether each channel's offset is removed.
    /// </summary>
    internal readonly record struct BandSelection(int Band, bool CommonMedianReference, bool AcCoupled);

    /// <summary>
    /// The control strip a <see cref="ProbeScopeVisualizer{TFrame}"/> draws, adding the band the probe
    /// is being viewed in to the standard columns.
    /// </summary>
    /// <remarks>
    /// Referencing and AC coupling sit here beside the band because each is a filter: referencing a spatial
    /// high-pass across the ADC group, AC coupling a temporal one on each channel. Turning either on defines a
    /// different signal exactly as choosing another band does, and all three rebind the chain.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type of the scope whose settings the strip shows.</typeparam>
    internal sealed class ImGuiProbeScopeControlStrip<TFrame> : ImGuiLfpViewerControlStrip
    {
        readonly ProbeScope<TFrame> scope;
        readonly BehaviorSubject<BandSelection> selection;

        /// <param name="scope">The node whose band, referencing and AC coupling the strip shows and sets.</param>
        public ImGuiProbeScopeControlStrip(ProbeScope<TFrame> scope)
        {
            this.scope = scope;
            selection = new(Current);
        }

        /// <summary>
        /// The bands to offer, in display order. The owner sets this once the probe is known.
        /// </summary>
        public IReadOnlyList<ProbeScopeBand> Bands { get; set; } = Array.Empty<ProbeScopeBand>();

        /// <summary>
        /// The signal being asked for, starting with whatever the strip opens on.
        /// </summary>
        public IObservable<BandSelection> BandSelected => selection;

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

        private protected override int ParameterColumns => base.ParameterColumns + 2;

        private protected override void LeadingColumns(ImGuiLfpViewerPanel panel)
        {
            // NB: a change while paused would not show until resumed.
            ImGui.BeginDisabled(panel.Paused);

            ImGui.SetNextItemWidth(MenuColumn("Band"));
            BandCombo();

            MenuColumn("Filters");
            var cmr = scope.CommonMedianReference;
            if (ImGui.Checkbox("CMR", ref cmr))
                scope.CommonMedianReference = cmr;
            ImGui.SetItemTooltip("Reference each channel against the median of the channels sharing its converter");

            ImGui.SameLine();
            var band = Band;
            var acOffered = band >= Bands.Count || Bands[band].AcDescription is not null;
            ImGui.BeginDisabled(!acOffered);

            // NB: shown unchecked where the band does not offer it, since nothing is applied there, while the
            // choice is kept for the next band that does.
            var ac = scope.AcCoupled && acOffered;
            if (ImGui.Checkbox("AC", ref ac))
                scope.AcCoupled = ac;
            ImGui.SetItemTooltip("High-pass each channel at 1 Hz, as AC coupling does on a scope");
            ImGui.EndDisabled();

            ImGui.EndDisabled();
        }

        void BandCombo()
        {
            var band = Band;
            var singleBand = Bands.Count <= 1;
            var preview = band >= 0 && band < Bands.Count ? Bands[band].Name : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##band", preview))
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
