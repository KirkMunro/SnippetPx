using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace SnippetPx
{
    [Cmdlet(
        VerbsLifecycle.Invoke,
        "Snippet"
    )]
    [OutputType(typeof(Object))]
    public class InvokeSnippetCommand : SnippetCommand
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the snippet."
        )]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The parameters that will be passed into the snippet."
        )]
        [ValidateNotNullOrEmpty()]
        public Hashtable Parameters { get; set; }

        [Parameter(
            HelpMessage = "If true, invoke the snippet in a child scope; otherwise, invoke the snippet in the current scope."
        )]
        public SwitchParameter ChildScope { get; set; }

        protected override void ProcessRecord()
        {
            if (!snippetsDirectory.Contains(Name))
            {
                throw new ItemNotFoundException("Snippet \"" + Name + "\" was not found.");
            }
            string snippetPath = snippetsDirectory[Name] as string;
            if (!File.Exists(snippetPath))
            {
                throw new FileNotFoundException("The file associated with snippet \"" + Name + "\" was not found.", snippetPath);
            }
            string script = "";
            List<string> missingMandatoryParameters = new List<string>();
            PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Get-Command");
            ps.AddParameter("Name", snippetPath);
            var results = ps.Invoke();
            if (ps.HadErrors)
            {
                foreach (ErrorRecord error in ps.Streams.Error)
                {
                    WriteError(error);
                }
            }
            else
            {
                foreach (PSObject psObject in results)
                {
                    ScriptBlock scriptBlock = psObject.Properties["ScriptBlock"].Value as ScriptBlock;
                    script = scriptBlock.ToString();
                    ScriptBlockAst ast = scriptBlock.Ast as ScriptBlockAst;
                    if (ast.ParamBlock != null)
                    {
                        foreach (ParameterAst parameterAst in ast.ParamBlock.Parameters)
                        {
                            string parameterName = parameterAst.Name.VariablePath.UserPath;
                            if ((parameterAst.DefaultValue == null) && (!MyInvocation.BoundParameters.ContainsKey("Parameters") || !Parameters.ContainsKey(parameterName)))
                            {
                                missingMandatoryParameters.Add(parameterName);
                            }
                        }
                    }
                }
                if (missingMandatoryParameters.Count > 0)
                {
                    throw new ParameterBindingException("The following mandatory parameters were not provided in the invocation of the \"" + Name + "\" snippet: " + String.Join(",", missingMandatoryParameters.ToArray()) + ".");
                }
                WriteVerbose("Invoking snippet \"" + Name + "\" (" + snippetPath + ").");
                ps.Commands.Clear();
                ps.AddScript(script, MyInvocation.BoundParameters.ContainsKey("ChildScope") && ChildScope.IsPresent);
                if (MyInvocation.BoundParameters.ContainsKey("Parameters") && (Parameters.Count > 0))
                {
                    ps.AddParameters(Parameters);
                }
                results = ps.Invoke();
                if (ps.HadErrors)
                {
                    foreach (ErrorRecord error in ps.Streams.Error)
                    {
                        WriteError(error);
                    }
                }
                else
                {
                    foreach (PSObject psObject in results)
                    {
                        WriteObject(psObject);
                    }
                }
            }
        }
    }
}