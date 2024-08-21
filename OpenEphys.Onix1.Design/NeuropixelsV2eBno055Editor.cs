﻿using System;
using System.ComponentModel;
using Bonsai.Design;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2eBno055"/>.
    /// </summary>
    public class NeuropixelsV2eBno055Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2eBno055 configureBno055)
                {
                    using var editorDialog = new NeuropixelsV2eBno055Dialog(configureBno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureBno055.Enable = editorDialog.ConfigureNode.Enable;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}