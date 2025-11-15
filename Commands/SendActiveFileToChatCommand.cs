using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows;
using EnvDTE;
using LocalLLMChatVS.Services;
using LocalLLMChatVS.ToolWindows;
using LocalLLMChatVS.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS.Commands
{
    /// <summary>
    /// Command to send the active file to chat
    /// </summary>
    internal sealed class SendActiveFileToChatCommand
    {
        public const int CommandId = PackageIds.SendActiveFileToChatCommand;
        public static readonly Guid CommandSet = new Guid(PackageGuids.LocalLLMChatPackageCmdSetString);
        private readonly AsyncPackage package;

        private SendActiveFileToChatCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static SendActiveFileToChatCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SendActiveFileToChatCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument == null)
                {
                    MessageBox.Show("No active document found.", "No Active File", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string filePath = dte.ActiveDocument.FullName;
                if (string.IsNullOrEmpty(filePath))
                {
                    MessageBox.Show("Active document has no file path.", "No Active File", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get relative path
                var workspaceService = new WorkspaceService();
                string solutionDir = await workspaceService.GetSolutionDirectoryAsync();
                string relativePath = PathValidator.GetRelativePath(solutionDir, filePath);

                // Get content
                string content = File.ReadAllText(filePath);

                // Open chat window
                ToolWindowPane window = package.FindToolWindow(typeof(ChatWindow), 0, true);
                if (window?.Frame != null)
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((IVsWindowFrame)window.Frame).Show());

                    var control = (ChatWindowControl)window.Content;
                    if (control != null)
                    {
                        string message = $"Here is the content of file \"{relativePath}\":\n\n```\n{content}\n```";
                        control.SendMessageToChat(message);

                        MessageBox.Show($"Sent \"{relativePath}\" to chat", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
