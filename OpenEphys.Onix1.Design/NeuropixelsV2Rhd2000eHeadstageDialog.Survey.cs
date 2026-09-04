using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenEphys.Onix1.Design
{
    internal partial class NeuropixelsV2Rhd2000eHeadstageDialog
    {
        const float SurveyPanelWidth = 340f;

        void InitializeControlPanel(PortName initialPort, Action<PortName> setHeadstagePort, Action<double?> setHeadstagePortVoltage)
        {
            var targets = new List<NeuropixeslV2eSurveyTarget>
            {
                new(nameof(ConfigureHeadstageNeuropixelsV2Rhd2000e.NeuropixelsV2), DialogNeuropixelsV2,
                    hs => ((ConfigureHeadstageNeuropixelsV2Rhd2000e)hs).NeuropixelsV2),
            };

            var panel = new NeuropixelsV2eHeadstageControlPanel(targets, BuildSurveyHeadstage, initialPort, setHeadstagePort, setHeadstagePortVoltage, Log);
            SetSidePanel(panel, SurveyPanelWidth, "Headstage Control");
            ClientSize = new Size(ClientSize.Width + (int)SurveyPanelWidth, ClientSize.Height);
        }

        static (MultiDeviceFactory Headstage, AutoPortVoltage PortVoltage) BuildSurveyHeadstage(PortName port, double? portVoltage)
        {
            var headstage = new ConfigureHeadstageNeuropixelsV2Rhd2000e
            {
                Name = NeuropixelsV2eHeadstageSurveyRunner.GroupName,
                Port = port,
                PortVoltage = new AutoPortVoltage(portVoltage)
            };
            return (headstage, headstage.PortVoltage);
        }
    }
}
