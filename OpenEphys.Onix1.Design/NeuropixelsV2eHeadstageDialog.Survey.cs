using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenEphys.Onix1.Design
{
    internal partial class NeuropixelsV2eHeadstageDialog
    {
        const float SurveyPanelWidth = 340f;

        void InitializeControlPanel(PortName initialPort, Action<PortName> setHeadstagePort, Action<double?> setHeadstagePortVoltage)
        {
            var targets = new List<NeuropixeslV2eSurveyTarget>
            {
                new(nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2A), DialogNeuropixelsV2A,
                    hs => ((ConfigureHeadstageNeuropixelsV2e)hs).NeuropixelsV2A),
                new(nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2B), DialogNeuropixelsV2B,
                    hs => ((ConfigureHeadstageNeuropixelsV2e)hs).NeuropixelsV2B),
            };

            var panel = new NeuropixelsV2eHeadstageControlPanel(targets, BuildSurveyHeadstage, initialPort, setHeadstagePort, setHeadstagePortVoltage, Log);
            SetSidePanel(panel, SurveyPanelWidth, "Headstage Control");
            ClientSize = new Size(ClientSize.Width + (int)SurveyPanelWidth, ClientSize.Height);
        }

        // TODO: always builds a plain ConfigureHeadstageNeuropixelsV2e survey pipeline, even for the Beta
        // ctor overload. This may not be correct.
        static (MultiDeviceFactory Headstage, AutoPortVoltage PortVoltage) BuildSurveyHeadstage(PortName port, double? portVoltage)
        {
            var headstage = new ConfigureHeadstageNeuropixelsV2e
            {
                Name = NeuropixelsV2eHeadstageSurveyRunner.GroupName,
                Port = port,
                PortVoltage = new AutoPortVoltage(portVoltage)
            };
            return (headstage, headstage.PortVoltage);
        }
    }
}
