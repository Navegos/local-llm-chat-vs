using System;
using System.ComponentModel.Design;
using System.Windows;
using LocalLLMChatVS.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS.Commands
{
    /// <summary>
    /// Command to clear the conversation
    /// </summary>
    internal sealed class ClearConversationCommand
    {
        public const int CommandId = PackageIds.ClearConversationCommand;
        public static readonly Guid CommandSet = new Guid(PackageGuids.LocalLLMChatPackageCmdSetString);
        private readonly AsyncPackage package;

        private ClearConversationCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ClearConversationCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ClearConversationCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            MessageBox.Show("Please use the Clear button in the chat window.", "Local LLM Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
