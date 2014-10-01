using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace SnippetPx
{
    public class SnippetCommand : PSCmdlet
    {
        protected OrderedDictionary snippetsDirectory = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

        protected override void BeginProcessing()
        {
            List<string> snippetPaths = new List<string>();
            // Add the current user snippets path if it exists (Documents\WindowsPowerShell\snippets)
            string cuSnippetsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindowsPowerShell", "snippets");
            if (Directory.Exists(cuSnippetsPath))
            {
                snippetPaths.Add(cuSnippetsPath);
            }
            // Add the all users snippets path if it exists (Program Files\WindowsPowerShell\snippets)
            string auSnippetsPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "WindowsPowerShell", "snippets");
            if (Directory.Exists(auSnippetsPath))
            {
                snippetPaths.Add(auSnippetsPath);
            }
            // Add the SnippetPx module snippets path if it exists (SnippetPx\snippets)
            string snippetPxSnippetsPath = Path.Combine(Path.GetDirectoryName(typeof(SnippetCommand).Assembly.Location), "snippets");
            if (Directory.Exists(snippetPxSnippetsPath))
            {
                snippetPaths.Add(snippetPxSnippetsPath);
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
                        if (Directory.Exists(Path.Combine(directory, directoryName + ".psd1")) ||
                            Directory.Exists(Path.Combine(directory, directoryName + ".psm1")) ||
                            Directory.Exists(Path.Combine(directory, directoryName + ".dll")))
                        {
                            string moduleSnippetsPath = Path.Combine(directory, "snippets");
                            if (Directory.Exists(moduleSnippetsPath) &&
                                (snippetPaths.IndexOf(moduleSnippetsPath) == -1))
                            {
                                snippetPaths.Add(Path.Combine(directory, "snippets"));
                            }
                        }
                    }
                }
            }
            // Build the snippet directory from the first unique snippet (name) among all of the snippet paths, processed in order
            foreach (string snippetsPath in snippetPaths)
            {
                foreach (string filePath in Directory.EnumerateFiles(snippetsPath, "*.ps1", SearchOption.AllDirectories))
                {
                    string snippetName = Path.GetFileNameWithoutExtension(filePath);
                    if (!snippetsDirectory.Contains(snippetName))
                    {
                        snippetsDirectory.Add(snippetName, filePath);
                    }
                }
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
