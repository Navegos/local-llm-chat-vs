using System;
using System.ComponentModel.Design;
using System.Windows;
using LocalLLMChatVS.Services;
using LocalLLMChatVS.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS.Commands
{
    /// <summary>
    /// Command to send a file to chat
    /// </summary>
    internal sealed class SendFileToChatCommand
    {
        public const int CommandId = PackageIds.SendFileToChatCommand;
        public static readonly Guid CommandSet = new Guid(PackageGuids.LocalLLMChatPackageCmdSetString);
        private readonly AsyncPackage package;

        private SendFileToChatCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static SendFileToChatCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SendFileToChatCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Simple input dialog
            string filePath = Microsoft.VisualBasic.Interaction.InputBox("Enter relative path to file:", "Send File to Chat", "src/Program.cs");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            _ = ExecuteAsync(filePath);
        }

        private async Task ExecuteAsync(string filePath)
        {
            try
            {
                var workspaceService = new WorkspaceService();

                // Read file
                string content = await workspaceService.ReadFileAsync(filePath);

                // Open chat window
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                ToolWindowPane window = package.FindToolWindow(typeof(ChatWindow), 0, true);
                if (window?.Frame != null)
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame).Show());

                    var control = (ChatWindowControl)window.Content;
                    if (control != null)
                    {
                        string message = $"Here is the content of file \"{filePath}\":\n\n```\n{content}\n```";
                        control.SendMessageToChat(message);

                        MessageBox.Show($"Sent \"{filePath}\" to chat", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
