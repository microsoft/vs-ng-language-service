// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;

namespace AngularLanguageService
{
    [Export(typeof(AngularLanguageServiceOutputPane))]
    public class AngularLanguageServiceOutputPane
    {
        private AsyncLazy<bool> initializePane;
        private Guid paneGuid;

        public AngularLanguageServiceOutputPane()
        {
            initializePane = new AsyncLazy<bool>(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsOutputWindow output = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));

                // Create a new pane.
                output.CreatePane(
                    ref paneGuid,
                    "Angular Language Service",
                    Convert.ToInt32(true),
                    Convert.ToInt32(true));

                return true;
            }, ThreadHelper.JoinableTaskFactory);
        }

        public async System.Threading.Tasks.Task WriteAsync(string s)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await initializePane.GetValueAsync();

            IVsOutputWindow output = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));

            // Retrieve the new pane.
            output.GetPane(ref paneGuid, out var pane);

            pane.OutputString($"{s}\n");
        }
    }
}