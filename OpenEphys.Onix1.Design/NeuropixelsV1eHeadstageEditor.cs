﻿using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1e configureHeadstage)
                {
                    using var editorDialog = new NeuropixelsV1eHeadstageDialog(configureHeadstage.NeuropixelsV1e, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureHeadstage.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        configureHeadstage.NeuropixelsV1e.Enable = editorDialog.DialogNeuropixelsV1e.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV1e.ProbeConfiguration = editorDialog.DialogNeuropixelsV1e.ConfigureNode.ProbeConfiguration;
                        configureHeadstage.NeuropixelsV1e.InvertPolarity = editorDialog.DialogNeuropixelsV1e.ConfigureNode.InvertPolarity;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
