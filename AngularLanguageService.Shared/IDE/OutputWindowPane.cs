using System;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace AngularLanguageService.Shared.IDE
{
	/// <summary>
	/// Facilitates the interaction with panes in the Output window.
	/// </summary>
	internal sealed class OutputWindowPane
	{
		private readonly Guid guid;
		private readonly string paneName;
		private IVsOutputWindowPane? pane;

		private OutputWindowPane(Guid guid, string paneName)
		{
			this.guid = guid;
			this.paneName = paneName;
		}

		/// <summary>
		/// Creates a new Output window pane with the given name (title).
		/// </summary>
		/// <remarks>
		/// The new pane will be lazily created upon first write.
		/// </remarks>
		/// <param name="paneName">The name (title) of the new pane</param>
		/// <returns>A new OutputWindowPane.</returns>
		internal static OutputWindowPane Create(string paneName) => new(Guid.NewGuid(), paneName);

		/// <summary>
		/// Writes the given text followed by a new line to the Output window pane.
		/// </summary>
		/// <param name="message">The text value to write. May be an empty string, in which case a new line is written.</param>
		internal void WriteLineFireAndForget(string message)
		{
			_ = WriteLineAsync(message);
		}

		/// <summary>
		/// Writes the given text followed by a new line to the Output window pane.
		/// </summary>
		/// <param name="message">The text value to write. May be an empty string, in which case a new line is written.</param>
		internal async Task WriteLineAsync(string message)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			await InitializePaneAsync();

			if (pane is IVsOutputWindowPaneNoPump noPump)
			{
				noPump.OutputStringNoPump(message + Environment.NewLine);
			}
			else
			{
				ErrorHandler.ThrowOnFailure(pane!.OutputStringThreadSafe(message + Environment.NewLine));
			}
		}

		/// <summary>
		/// Ensures that the output pane has been created.
		/// </summary>
		private async Task InitializePaneAsync()
		{
			if (pane is null)
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				IVsOutputWindow outputWindow = (IVsOutputWindow) (await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsOutputWindow)))!;
				Guid paneGuid = guid;

				ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref paneGuid, paneName, fInitVisible: 1, fClearWithSolution: 1));
				ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref paneGuid, out pane));
				Assumes.Present(pane);
			}
		}
	}
}
