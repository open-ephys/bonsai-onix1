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
    internal sealed class ImGuiProbeScopeControlStrip : ImGuiLfpViewerControlStrip
    {
        readonly BehaviorSubject<BandSelection> selection = new(new BandSelection(0, CommonMedianReference: false, AcCoupled: false));

        /// <summary>
        /// The bands to offer, in display order. The owner sets this once the probe is known.
        /// </summary>
        public IReadOnlyList<ProbeScopeBand> Bands { get; set; } = Array.Empty<ProbeScopeBand>();

        /// <summary>
        /// The signal being asked for, starting with whatever the strip opens on.
        /// </summary>
        public IObservable<BandSelection> BandSelected => selection;

        private protected override int ParameterColumns => base.ParameterColumns + 2;

        private protected override void LeadingColumns(ImGuiLfpViewerPanel panel)
        {
            // NB: a change while paused would not show until resumed.
            ImGui.BeginDisabled(panel.Paused);

            ImGui.SetNextItemWidth(MenuColumn("Band"));
            BandCombo();

            MenuColumn("Filters");
            var cmr = selection.Value.CommonMedianReference;
            if (ImGui.Checkbox("CMR", ref cmr))
                selection.OnNext(selection.Value with { CommonMedianReference = cmr });
            ImGui.SetItemTooltip("Reference each channel against the median of the channels sharing its converter");

            ImGui.SameLine();
            var band = selection.Value.Band;
            ImGui.BeginDisabled(band < Bands.Count && Bands[band].AcDescription is null);
            var ac = selection.Value.AcCoupled;
            if (ImGui.Checkbox("AC", ref ac))
                selection.OnNext(selection.Value with { AcCoupled = ac });
            ImGui.SetItemTooltip("High-pass each channel at 1 Hz, as AC coupling does on a scope");
            ImGui.EndDisabled();

            ImGui.EndDisabled();
        }

        void BandCombo()
        {
            var band = selection.Value.Band;
            var singleBand = Bands.Count <= 1;
            var preview = band >= 0 && band < Bands.Count ? Bands[band].Name : string.Empty;

            if (singleBand) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("##band", preview))
            {
                for (int i = 0; i < Bands.Count; i++)
                {
                    var isSelected = i == band;
                    var passband = selection.Value.AcCoupled && Bands[i].AcDescription is not null
                        ? Bands[i].AcDescription
                        : Bands[i].Description;
                    if (ImGui.Selectable($"{Bands[i].Name}: {passband}", isSelected) && !isSelected)
                        selection.OnNext(selection.Value with { Band = i });
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (singleBand) ImGui.EndDisabled();
        }
    }
}
