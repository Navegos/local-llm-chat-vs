using System;
using System.ComponentModel.Design;
using LocalLLMChatVS.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS.Commands
{
    /// <summary>
    /// Command to open the chat window
    /// </summary>
    internal sealed class OpenChatCommand
    {
        public const int CommandId = PackageIds.OpenChatCommand;
        public static readonly Guid CommandSet = new Guid(PackageGuids.LocalLLMChatPackageCmdSetString);
        private readonly AsyncPackage package;

        private OpenChatCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static OpenChatCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenChatCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ToolWindowPane window = this.package.FindToolWindow(typeof(ChatWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame).Show());
        }
    }
}
