using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenEphys.Onix1.Design
{
    internal partial class NeuropixelsV1eHeadstageDialog
    {
        const float SurveyPanelWidth = 340f;

        void InitializeSurveyPanel(PortName initialPort, Action<PortName> setHeadstagePort, Action<double?> setHeadstagePortVoltage)
        {
            var targets = new List<NeuropixelsV1SurveyTarget>
            {
                new(nameof(ConfigureHeadstageNeuropixelsV1e.NeuropixelsV1), DialogNeuropixelsV1e,
                    hs => ((ConfigureHeadstageNeuropixelsV1e)hs).NeuropixelsV1),
            };

            var panel = new NeuropixelsV1HeadstageSurveyPanel(targets, BuildSurveyHeadstage, initialPort, setHeadstagePort, setHeadstagePortVoltage, Log);
            SetSidePanel(panel, SurveyPanelWidth, "Electrode survey");
            ClientSize = new Size(ClientSize.Width + (int)SurveyPanelWidth, ClientSize.Height);
        }

        static (MultiDeviceFactory Headstage, AutoPortVoltage PortVoltage) BuildSurveyHeadstage(PortName port, double? portVoltage)
        {
            var headstage = new ConfigureHeadstageNeuropixelsV1e
            {
                Name = NeuropixelsV1HeadstageSurveyRunner.GroupName,
                Port = port,
                PortVoltage = new AutoPortVoltage(portVoltage)
            };
            return (headstage, headstage.PortVoltage);
        }
    }
}
