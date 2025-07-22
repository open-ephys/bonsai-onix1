﻿using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1fHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1f configureHeadstage)
                {
                    using var editorDialog = new NeuropixelsV1fHeadstageDialog(configureHeadstage.NeuropixelsV1A, configureHeadstage.NeuropixelsV1B, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureHeadstage.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        configureHeadstage.NeuropixelsV1A.AdcCalibrationFile = editorDialog.DialogNeuropixelsV1A.ConfigureNode.AdcCalibrationFile;
                        configureHeadstage.NeuropixelsV1A.GainCalibrationFile = editorDialog.DialogNeuropixelsV1A.ConfigureNode.GainCalibrationFile;
                        configureHeadstage.NeuropixelsV1A.Enable = editorDialog.DialogNeuropixelsV1A.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV1A.ProbeConfiguration = editorDialog.DialogNeuropixelsV1A.ConfigureNode.ProbeConfiguration;
                        configureHeadstage.NeuropixelsV1A.InvertPolarity = editorDialog.DialogNeuropixelsV1A.ConfigureNode.InvertPolarity;

                        ProbeGroupHelper.SaveExternalProbeConfigurationFile(editorDialog.DialogNeuropixelsV1A.GetProbeGroup(),
                            configureHeadstage.NeuropixelsV1A.ProbeConfiguration.ProbeInterfaceFile);

                        configureHeadstage.NeuropixelsV1B.AdcCalibrationFile = editorDialog.DialogNeuropixelsV1B.ConfigureNode.AdcCalibrationFile;
                        configureHeadstage.NeuropixelsV1B.GainCalibrationFile = editorDialog.DialogNeuropixelsV1B.ConfigureNode.GainCalibrationFile;
                        configureHeadstage.NeuropixelsV1B.Enable = editorDialog.DialogNeuropixelsV1B.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV1B.ProbeConfiguration = editorDialog.DialogNeuropixelsV1B.ConfigureNode.ProbeConfiguration;
                        configureHeadstage.NeuropixelsV1B.InvertPolarity = editorDialog.DialogNeuropixelsV1B.ConfigureNode.InvertPolarity;

                        ProbeGroupHelper.SaveExternalProbeConfigurationFile(editorDialog.DialogNeuropixelsV1B.GetProbeGroup(),
                            configureHeadstage.NeuropixelsV1B.ProbeConfiguration.ProbeInterfaceFile);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
