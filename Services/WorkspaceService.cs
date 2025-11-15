using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using LocalLLMChatVS.Utilities;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS.Services
{
    /// <summary>
    /// Service for workspace and file operations
    /// </summary>
    public class WorkspaceService
    {
        /// <summary>
        /// Gets the active solution directory
        /// </summary>
        public async Task<string> GetSolutionDirectoryAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await LocalLLMChatPackage.Instance.GetServiceAsync(typeof(DTE)) as DTE;
            if (dte?.Solution == null)
            {
                throw new InvalidOperationException("No solution is currently open");
            }

            string solutionPath = dte.Solution.FullName;
            if (string.IsNullOrEmpty(solutionPath))
            {
                throw new InvalidOperationException("Solution path is not available");
            }

            return Path.GetDirectoryName(solutionPath);
        }

        /// <summary>
        /// Reads a file from the workspace
        /// </summary>
        public async Task<string> ReadFileAsync(string relativePath)
        {
            PathValidator.ValidateRelativePath(relativePath);

            string solutionDir = await GetSolutionDirectoryAsync();
            string fullPath = Path.Combine(solutionDir, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {relativePath}");
            }

            // Security check: ensure file is within solution directory
            if (!PathValidator.IsPathWithinDirectory(fullPath, solutionDir))
            {
                throw new UnauthorizedAccessException($"Access to file outside solution directory is not allowed: {relativePath}");
            }

            return await Task.Run(() => File.ReadAllText(fullPath, Encoding.UTF8));
        }

        /// <summary>
        /// Writes content to a file in the workspace
        /// </summary>
        public async Task WriteFileAsync(string relativePath, string content, int maxFileSize)
        {
            PathValidator.ValidateRelativePath(relativePath);
            SecurityValidator.ValidateFileContent(content, maxFileSize);

            string solutionDir = await GetSolutionDirectoryAsync();
            string fullPath = Path.Combine(solutionDir, relativePath);

            // Security check: ensure file is within solution directory
            if (!PathValidator.IsPathWithinDirectory(fullPath, solutionDir))
            {
                throw new UnauthorizedAccessException($"Cannot write file outside solution directory: {relativePath}");
            }

            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await Task.Run(() => File.WriteAllText(fullPath, content, Encoding.UTF8));
        }

        /// <summary>
        /// Lists files in a directory
        /// </summary>
        public async Task<List<FileEntry>> ListFilesAsync(string relativePath = "", bool recursive = false, int maxDepth = 3)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {
                PathValidator.ValidateRelativePath(relativePath);
            }

            string solutionDir = await GetSolutionDirectoryAsync();
            string fullPath = string.IsNullOrEmpty(relativePath)
                ? solutionDir
                : Path.Combine(solutionDir, relativePath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
            }

            return await Task.Run(() => ListFilesRecursive(fullPath, solutionDir, recursive, maxDepth, 0));
        }

        private List<FileEntry> ListFilesRecursive(string currentPath, string basePath, bool recursive, int maxDepth, int currentDepth)
        {
            var results = new List<FileEntry>();

            try
            {
                // Get files
                var files = Directory.GetFiles(currentPath)
                    .Where(f => !Path.GetFileName(f).StartsWith("."))
                    .Select(f => new FileEntry
                    {
                        Name = Path.GetFileName(f),
                        RelativePath = GetRelativePath(basePath, f),
                        Type = FileEntryType.File
                    });

                results.AddRange(files);

                // Get directories
                var directories = Directory.GetDirectories(currentPath)
                    .Where(d =>
                    {
                        string dirName = Path.GetFileName(d);
                        return !dirName.StartsWith(".") &&
                               dirName != "bin" &&
                               dirName != "obj" &&
                               dirName != "node_modules" &&
                               dirName != "__pycache__";
                    })
                    .Select(d => new FileEntry
                    {
                        Name = Path.GetFileName(d),
                        RelativePath = GetRelativePath(basePath, d),
                        Type = FileEntryType.Directory
                    });

                results.AddRange(directories);

                // Recursive listing
                if (recursive && currentDepth < maxDepth)
                {
                    foreach (var dir in directories)
                    {
                        string fullDirPath = Path.Combine(basePath, dir.RelativePath);
                        results.AddRange(ListFilesRecursive(fullDirPath, basePath, recursive, maxDepth, currentDepth + 1));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
            }

            return results.OrderBy(e => e.Type).ThenBy(e => e.Name).ToList();
        }

        /// <summary>
        /// Searches for files matching a pattern
        /// </summary>
        public async Task<List<string>> SearchFilesAsync(string pattern, int maxResults = 100)
        {
            string solutionDir = await GetSolutionDirectoryAsync();

            return await Task.Run(() =>
            {
                try
                {
                    var files = Directory.GetFiles(solutionDir, pattern, SearchOption.AllDirectories)
                        .Where(f =>
                        {
                            string relativePath = GetRelativePath(solutionDir, f);
                            return !relativePath.Contains("\\bin\\") &&
                                   !relativePath.Contains("\\obj\\") &&
                                   !relativePath.Contains("\\node_modules\\") &&
                                   !relativePath.Contains("\\.git\\");
                        })
                        .Take(maxResults)
                        .Select(f => GetRelativePath(solutionDir, f))
                        .ToList();

                    return files;
                }
                catch
                {
                    return new List<string>();
                }
            });
        }

        /// <summary>
        /// Gets workspace metadata
        /// </summary>
        public async Task<WorkspaceMetadata> GetWorkspaceMetadataAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await LocalLLMChatPackage.Instance.GetServiceAsync(typeof(DTE)) as DTE;
            if (dte?.Solution == null)
            {
                throw new InvalidOperationException("No solution is currently open");
            }

            string solutionPath = dte.Solution.FullName;
            string solutionDir = Path.GetDirectoryName(solutionPath);
            string solutionName = Path.GetFileNameWithoutExtension(solutionPath);

            var metadata = new WorkspaceMetadata
            {
                Name = solutionName,
                Path = solutionDir,
                HasGit = Directory.Exists(Path.Combine(solutionDir, ".git")),
                HasPackageJson = File.Exists(Path.Combine(solutionDir, "package.json"))
            };

            return metadata;
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
            Uri fullUri = new Uri(fullPath);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', '\\'));
        }
    }

    public class FileEntry
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public FileEntryType Type { get; set; }
    }

    public enum FileEntryType
    {
        File,
        Directory
    }

    public class WorkspaceMetadata
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool HasGit { get; set; }
        public bool HasPackageJson { get; set; }
    }
}
