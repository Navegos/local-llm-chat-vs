using System;
using System.Text;

namespace LocalLLMChatVS.Utilities
{
    /// <summary>
    /// Provides security validation for file content and other security-sensitive operations
    /// </summary>
    public static class SecurityValidator
    {
        /// <summary>
        /// Validates file content size
        /// </summary>
        /// <param name="content">The content to validate</param>
        /// <param name="maxSize">Maximum size in bytes</param>
        /// <exception cref="ArgumentException">Thrown if content exceeds size limit</exception>
        public static void ValidateFileContent(string content, int maxSize)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            int byteSize = Encoding.UTF8.GetByteCount(content);

            if (byteSize > maxSize)
            {
                throw new ArgumentException(
                    $"Content size ({FormatBytes(byteSize)}) exceeds maximum allowed size ({FormatBytes(maxSize)})");
            }
        }

        /// <summary>
        /// Validates URL format
        /// </summary>
        public static bool ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            try
            {
                Uri uri = new Uri(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats bytes into human-readable string
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 Bytes";

            string[] sizes = { "Bytes", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{Math.Round(size, 2)} {sizes[order]}";
        }
    }
}
