using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using Bonsai;
using Bonsai.Expressions;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides the pass-through node a waveform viewer opens on, holding the display settings every waveform
    /// viewer shares, which are saved with the workflow.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The viewer reads and writes these properties directly, as the property grid does, so the node holds the only
    /// copy of them.
    /// </para>
    /// <para>
    /// Only settings every viewer has belong here. One that a viewer would replace with its own, as ProbeScope's
    /// groups replace the LFP viewer's index-based ones, belongs to that viewer's node, so nothing here is ever
    /// hidden or overridden.
    /// </para>
    /// </remarks>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class WaveformScope : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the number of seconds of data retained behind the display.
        /// </summary>
        /// <remarks>
        /// While the display is paused, the view can be moved back through this much of what has already been
        /// shown and redrawn at any timebase.
        /// </remarks>
        [DefaultValue(10.0)]
        [Description("The number of seconds of data retained behind the display, which a paused view can " +
            "be moved back through. Longer histories cost proportionally more memory.")]
        public double HistorySeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of seconds the display spans.
        /// </summary>
        [DefaultValue(2.5)]
        [Description("The number of seconds the display spans.")]
        public double Timebase { get; set; } = 2.5;

        /// <summary>
        /// Gets or sets the amplitude spanned by one channel's row, in the units of the displayed data.
        /// </summary>
        [DefaultValue(500.0)]
        [Description("The amplitude spanned by one channel's row, in the units of the displayed data.")]
        public double RangeAmplitude { get; set; } = 500;

        /// <summary>
        /// Gets or sets the height in pixels of one channel's row.
        /// </summary>
        [DefaultValue(20f)]
        [Description("The height in pixels of one channel's row.")]
        public float ChannelHeight { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value specifying whether every channel is fitted to the display's height.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether every channel is fitted to the display's height, as far as the " +
            "smallest channel height allows.")]
        public bool FitChannels { get; set; }

        /// <summary>
        /// Gets or sets the palette the channels are drawn in.
        /// </summary>
        [DefaultValue(ColorPalette.OpenEphysGui)]
        [Description("The palette the channels are drawn in.")]
        public ColorPalette Palette { get; set; } = ColorPalette.OpenEphysGui;

        /// <summary>
        /// Gets or sets the color the custom palette is built from.
        /// </summary>
        [XmlIgnore]
        [Description("The color the custom palette is built from.")]
        public Color CustomColor { get; set; } = Color.FromArgb(0x8C, 0xDB, 0x8E);

        /// <summary>
        /// Gets or sets an HTML representation of the custom color, for serialization.
        /// </summary>
        [Browsable(false)]
        [DefaultValue("#8CDB8E")]
        [XmlElement(nameof(CustomColor))]
        public string CustomColorHtml
        {
            get => ColorTranslator.ToHtml(CustomColor);
            set => CustomColor = ColorTranslator.FromHtml(value);
        }

        /// <summary>
        /// Gets or sets a value specifying whether channels are drawn as rows of color rather than as traces.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether channels are drawn as rows of color rather than as traces.")]
        public bool ShowHeatmap { get; set; }

        /// <summary>
        /// Gets or sets the magnitude at or below which a row of color is black, in the units of the displayed
        /// data.
        /// </summary>
        [DefaultValue(0.0)]
        [Description("The magnitude at or below which a row of color is black, in the units of the displayed data.")]
        public double ColorThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether channels that clip are marked and a warning shown.
        /// </summary>
        [DefaultValue(true)]
        [Description("Specifies whether channels that clip are marked and a warning shown.")]
        public bool ShowClipping { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying whether the cursors are shown.
        /// </summary>
        [DefaultValue(false)]
        [Description("Specifies whether the cursors are shown.")]
        public bool ShowCursors { get; set; }
    }
}
