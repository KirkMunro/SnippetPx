using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;

namespace SnippetPx
{
    [Cmdlet(
        VerbsCommon.Get,
        "Snippet"
    )]
    [OutputType(typeof(Snippet))]
    public class GetSnippetCommand : SnippetCommand
    {
        [Parameter(
            Position = 0,
            HelpMessage = "The name of a snippet."
        )]
        [ValidateNotNullOrEmpty()]
        [SupportsWildcards()]
        public string[] Name { get; set; }

        protected override void ProcessRecord()
        {
            List<string> resultSet = new List<string>();
            if (MyInvocation.BoundParameters.ContainsKey("Name"))
            {
                List<WildcardPattern> patterns = new List<WildcardPattern>();
                List<string> names = new List<string>();
                foreach (string name in Name)
                {
                    if (WildcardPattern.ContainsWildcardCharacters(name))
                    {
                        patterns.Add(new WildcardPattern(name, WildcardOptions.IgnoreCase));
                    }
                    else
                    {
                        names.Add(name);
                    }
                }
                foreach (string key in snippetsDirectory.Keys)
                {
                    if (names.Contains(key, StringComparer.OrdinalIgnoreCase) ||
                        patterns.Any(p => p.IsMatch(key)))
                    {
                        resultSet.Add(key);
                    }
                }
            }
            else
            {
                foreach (string key in snippetsDirectory.Keys)
                {
                    resultSet.Add(key);
                }
            }
            foreach (string name in resultSet)
            {
                Snippet snippet = new Snippet(name, snippetsDirectory[name] as string);
                PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
                ps.AddCommand("Get-Help", true);
                ps.AddParameter("Name", snippetsDirectory[name] as string);
                foreach (PSObject psObject in ps.Invoke())
                {
                    if (psObject.BaseObject is PSCustomObject)
                    {
                        snippet.Synopsis = psObject.Properties["Synopsis"].Value as string;
                        ArrayList descriptionStrings = new ArrayList();
                        foreach (PSObject psDescriptionObject in psObject.Properties["Description"].Value as PSObject[])
                        {
                            descriptionStrings.Add(psDescriptionObject.Properties["Text"].Value);
                        }
                        snippet.Description = string.Join("\r\n", descriptionStrings.ToArray());
                    }
                }
                ps.Commands.Clear();
                ps.AddCommand("Get-Command", true);
                ps.AddParameter("Name", snippetsDirectory[name] as string);
                foreach (PSObject psObject in ps.Invoke())
                {
                    if (psObject.BaseObject is ExternalScriptInfo)
                    {
                        snippet.ScriptBlock = psObject.Properties["ScriptBlock"].Value as ScriptBlock;
                    }
                }
                WriteObject(snippet);
            }
        }
    }
}