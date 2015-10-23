using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace SnippetPx
{
    public class SnippetCommand : PSCmdlet
    {
        protected OrderedDictionary snippetsDirectory = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

        protected override void BeginProcessing()
        {
            List<string> snippetPaths = new List<string>();
            // Add the current user snippets path if it exists (Documents\WindowsPowerShell\snippets)
            AddSnippetsPathIfPresent(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindowsPowerShell", "snippets"), snippetPaths);
            // Add the all users snippets path if it exists (Program Files\WindowsPowerShell\snippets)
            AddSnippetsPathIfPresent(Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "WindowsPowerShell", "snippets"), snippetPaths);
            // Add the SnippetPx module snippets path if it exists (SnippetPx\snippets)
            AddSnippetsPathIfPresent(Path.Combine(Path.GetDirectoryName(typeof(SnippetCommand).Assembly.Location), "snippets"), snippetPaths);
            // Add snippets paths for modules that are already loaded first
            foreach (PSModuleInfo psModuleInfo in this.GetLoadedModules())
            {
                AddSnippetsPathIfPresent(psModuleInfo.ModuleBase, snippetPaths);
            }
            // Add all remaining snippets paths from other modules based on their order in PSModulePath
            string[] modulePaths = Environment.GetEnvironmentVariable("PSModulePath").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string modulePath in modulePaths)
            {
                if (Directory.Exists(modulePath))
                {
                    foreach (string directory in Directory.EnumerateDirectories(modulePath))
                    {
                        string directoryName = directory.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
                        if (LooksLikeModulePath(directory, directoryName))
                        {
                            AddSnippetsPathIfPresent(directory, snippetPaths);
                        }
                        else
                        {
                            foreach (string versionDirectory in Directory.EnumerateDirectories(directory).Where(x => Regex.IsMatch(x.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last(), @"^\d+(\.\d+){0,3}$")).OrderByDescending(x => new Version(x.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last())))
                            {
                                if (LooksLikeModulePath(versionDirectory, directoryName))
                                {
                                    AddSnippetsPathIfPresent(versionDirectory, snippetPaths);
                                }
                            }
                        }
                    }
                }
            }
            // Build the snippet directory from the first unique snippet (name) among all of the snippet paths, processed in order
            foreach (string snippetsPath in snippetPaths)
            {
                foreach (string filePath in Directory.EnumerateFiles(snippetsPath, "*.ps1", SearchOption.AllDirectories).Where(x => x.EndsWith(".ps1")))
                {
                    string snippetName = Path.GetFileNameWithoutExtension(filePath);
                    if (!snippetsDirectory.Contains(snippetName))
                    {
                        snippetsDirectory.Add(snippetName, filePath);
                    }
                }
            }
        }

        private static bool LooksLikeModulePath(string moduleDirectory, string directoryName)
        {
            return File.Exists(Path.Combine(moduleDirectory, directoryName + ".psd1")) ||
                   File.Exists(Path.Combine(moduleDirectory, directoryName + ".psm1")) ||
                   File.Exists(Path.Combine(moduleDirectory, directoryName + ".dll"));
        }

        private static void AddSnippetsPathIfPresent(string moduleDirectory, List<string> snippetPaths)
        {
            string snippetsPath = Path.Combine(moduleDirectory, "snippets");
            if (Directory.Exists(snippetsPath) &&
                (snippetPaths.IndexOf(snippetsPath) == -1))
            {
                snippetPaths.Add(snippetsPath);
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
