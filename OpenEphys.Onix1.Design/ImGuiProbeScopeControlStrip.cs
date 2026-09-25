using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Hexa.NET.ImGui;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The signal a probe scope is being asked to display: which band, and whether it is referenced
    /// against the median of the channels sharing its converter.
    /// </summary>
    internal readonly record struct BandSelection(int Band, bool CommonMedianReference);

    /// <summary>
    /// The control strip a <see cref="ProbeScopeVisualizer{TFrame}"/> draws, adding the band the probe
    /// is being viewed in to the standard columns.
    /// </summary>
    /// <remarks>
    /// Referencing sits here beside the band because it is a spatial high-pass across the ADC group:
    /// turning it on defines a different signal exactly as choosing another band does, and both rebind
    /// the chain.
    /// </remarks>
    internal sealed class ImGuiProbeScopeControlStrip : ImGuiLfpViewerControlStrip
    {
        readonly BehaviorSubject<BandSelection> selection = new(new BandSelection(0, false));

        /// <summary>
        /// The bands to offer, in display order, as a short name and a description of the passband. The
        /// owner sets this once the probe is known.
        /// </summary>
        public IReadOnlyList<(string Name, string Description)> Bands { get; set; } =
            Array.Empty<(string, string)>();

        /// <summary>
        /// The signal being asked for, starting with whatever the strip opens on.
        /// </summary>
        public IObservable<BandSelection> BandSelected => selection;

        private protected override int Columns => base.Columns + 1;

        private protected override void LeadingColumns(ImGuiLfpViewerPanel panel)
        {
            if (!BeginMenuColumn("Band"))
                return;

            ImGui.BeginDisabled(panel.Paused); // NB: changing would cause buffer refresh and unpause
            BandCombo();

            ImGui.SameLine();
            var cmr = selection.Value.CommonMedianReference;
            if (ImGui.Checkbox("CMR", ref cmr))
                selection.OnNext(selection.Value with { CommonMedianReference = cmr });

            ImGui.EndDisabled();
            EndMenuColumn();
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
                    var (name, description) = Bands[i];
                    if (ImGui.Selectable($"{name}: {description}", isSelected) && !isSelected)
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
