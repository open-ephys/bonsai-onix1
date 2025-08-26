﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public class NeuropixelsV2eEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is IConfigureNeuropixelsV2 configureNeuropixelsV2e)
                {
                    using var editorDialog = new NeuropixelsV2eDialog(configureNeuropixelsV2e);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNeuropixelsV2e.Enable = editorDialog.ConfigureNode.Enable;
                        configureNeuropixelsV2e.ProbeConfigurationA = editorDialog.ConfigureNode.ProbeConfigurationA;
                        configureNeuropixelsV2e.ProbeConfigurationB = editorDialog.ConfigureNode.ProbeConfigurationB;
                        configureNeuropixelsV2e.InvertPolarity = editorDialog.ConfigureNode.InvertPolarity;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
