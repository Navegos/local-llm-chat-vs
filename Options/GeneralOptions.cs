using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LocalLLMChatVS.Options
{
    /// <summary>
    /// Options page for configuring Local LLM Chat
    /// </summary>
    [ComVisible(true)]
    [Guid("4A5B6C7D-8E9F-4A5B-8C9D-0E1F2A3B4C5D")]
    public class GeneralOptions : DialogPage
    {
        [Category("API Configuration")]
        [DisplayName("API URL")]
        [Description("Full API endpoint URL. Examples:\n- OpenAI: https://api.openai.com/v1/chat/completions\n- Ollama: http://localhost:11434/v1/chat/completions\n- Custom: http://localhost:1234/v1/chat/completions")]
        [DefaultValue("http://localhost:11434/v1/chat/completions")]
        public string ApiUrl { get; set; } = "http://localhost:11434/v1/chat/completions";

        [Category("API Configuration")]
        [DisplayName("API Token")]
        [Description("API authentication token. Examples:\n- OpenAI: sk-your-openai-api-key-here\n- Ollama: ollama (or any dummy value)\n- Custom: your-token-or-dummy-value")]
        [DefaultValue("ollama")]
        [PasswordPropertyText(true)]
        public string ApiToken { get; set; } = "ollama";

        [Category("API Configuration")]
        [DisplayName("Model Name")]
        [Description("Model name to use. Examples:\n- OpenAI: gpt-4, gpt-3.5-turbo, gpt-4-turbo\n- Ollama: llama3.2, mistral, codellama\n- Custom: your-model-name")]
        [DefaultValue("llama3.2")]
        public string ModelName { get; set; } = "llama3.2";

        [Category("Model Parameters")]
        [DisplayName("Temperature")]
        [Description("Sampling temperature for model responses (0.0 = deterministic, 2.0 = very random)")]
        [DefaultValue(0.7)]
        public double Temperature { get; set; } = 0.7;

        [Category("Model Parameters")]
        [DisplayName("Max Tokens")]
        [Description("Maximum tokens for model responses")]
        [DefaultValue(2048)]
        public int MaxTokens { get; set; } = 2048;

        [Category("Model Parameters")]
        [DisplayName("System Prompt")]
        [Description("System prompt sent to the LLM to define its behavior")]
        [DefaultValue("You are a helpful coding assistant inside Visual Studio. Keep answers concise. When proposing file content, respond with a fenced code block beginning with ```file path=\"relative/path.ext\" followed by the complete file content.")]
        public string SystemPrompt { get; set; } = "You are a helpful coding assistant inside Visual Studio. Keep answers concise. When proposing file content, respond with a fenced code block beginning with ```file path=\"relative/path.ext\" followed by the complete file content.";

        [Category("Conversation")]
        [DisplayName("Max History Messages")]
        [Description("Maximum number of messages to keep in conversation history")]
        [DefaultValue(50)]
        public int MaxHistoryMessages { get; set; } = 50;

        [Category("Network")]
        [DisplayName("Request Timeout (ms)")]
        [Description("Request timeout in milliseconds (default: 120000 = 2 minutes)")]
        [DefaultValue(120000)]
        public int RequestTimeout { get; set; } = 120000;

        [Category("Security")]
        [DisplayName("Max File Size (bytes)")]
        [Description("Maximum file size in bytes for LLM-generated files (default: 1MB)")]
        [DefaultValue(1048576)]
        public int MaxFileSize { get; set; } = 1048576;

        [Category("Security")]
        [DisplayName("Allow Write Without Prompt")]
        [Description("If enabled, allow /write command to create/update files without confirmation. NOT recommended for security.")]
        [DefaultValue(false)]
        public bool AllowWriteWithoutPrompt { get; set; } = false;
    }
}
