using System;
using System.IO;
using System.Linq;

namespace LocalLLMChatVS.Utilities
{
    /// <summary>
    /// Validates file paths to prevent security issues like path traversal
    /// </summary>
    public static class PathValidator
    {
        /// <summary>
        /// Validates that a relative path doesn't escape the workspace
        /// </summary>
        /// <param name="relativePath">The relative path to validate</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid or attempts traversal</exception>
        public static void ValidateRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Path cannot be empty");
            }

            // Normalize the path to handle different separators
            string normalized = Path.GetFullPath(Path.Combine(".", relativePath));

            // Check for parent directory traversal
            if (normalized.Contains(".."))
            {
                throw new ArgumentException("Path traversal (..) is not allowed");
            }

            // Check if path is absolute
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("Absolute paths are not allowed");
            }

            // Check for suspicious characters (Windows)
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                char[] invalidChars = new char[] { '<', '>', ':', '"', '|', '?', '*' };
                if (relativePath.Any(c => invalidChars.Contains(c)))
                {
                    throw new ArgumentException("Path contains invalid characters");
                }
            }

            // Additional security: reject paths with null bytes
            if (relativePath.Contains("\0"))
            {
                throw new ArgumentException("Path contains null bytes");
            }
        }

        /// <summary>
        /// Validates that a file path is within a given base directory
        /// </summary>
        public static bool IsPathWithinDirectory(string filePath, string baseDirectory)
        {
            try
            {
                string fullFilePath = Path.GetFullPath(filePath);
                string fullBasePath = Path.GetFullPath(baseDirectory);

                return fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the relative path from one path to another (for .NET Framework compatibility)
        /// </summary>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
