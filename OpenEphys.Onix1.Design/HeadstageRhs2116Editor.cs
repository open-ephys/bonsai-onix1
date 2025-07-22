﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageRhs2116"/>.
    /// </summary>
    public class HeadstageRhs2116Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageRhs2116 configureNode)
                {
                    using var editorDialog = new HeadstageRhs2116Dialog(configureNode.StimulusTrigger.ProbeInterfaceFile,
                        configureNode.StimulusTrigger.StimulusSequence, configureNode.Rhs2116Pair);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.StimulusTrigger.StimulusSequence = editorDialog.StimulusSequenceDialog.Sequence;
                        configureNode.StimulusTrigger.ProbeInterfaceFile = editorDialog.StimulusSequenceDialog.ChannelDialog.ProbeInterfaceFile;
                        configureNode.Rhs2116Pair = editorDialog.Rhs2116Dialog.ConfigureNode;

                        ProbeGroupHelper.SaveExternalProbeConfigurationFile(editorDialog.StimulusSequenceDialog.ChannelDialog.ProbeGroup, configureNode.StimulusTrigger.ProbeInterfaceFile);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
