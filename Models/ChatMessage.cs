using System;

namespace LocalLLMChatVS.Models
{
    /// <summary>
    /// Represents a chat message in the conversation
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } // "system", "user", "assistant"
        public string Content { get; set; }

        public ChatMessage(string role, string content)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }

    /// <summary>
    /// File suggestion extracted from LLM response
    /// </summary>
    public class FileSuggestion
    {
        public string Path { get; set; }
        public string Content { get; set; }

        public FileSuggestion(string path, string content)
        {
            Path = path;
            Content = content;
        }
    }
}
