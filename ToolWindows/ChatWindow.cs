using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace LocalLLMChatVS.ToolWindows
{
    /// <summary>
    /// Tool window for Local LLM Chat
    /// </summary>
    [Guid("5B6C7D8E-9F0A-4B5C-8D9E-0F1A2B3C4D5E")]
    public class ChatWindow : ToolWindowPane
    {
        public ChatWindow() : base(null)
        {
            this.Caption = "Local LLM Chat";
            this.BitmapImageMoniker = KnownMonikers.CommentGroup;
            this.Content = new ChatWindowControl();
        }
    }
}
