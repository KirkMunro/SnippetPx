using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

namespace SnippetPx
{
    [Cmdlet(
        VerbsLifecycle.Invoke,
        "Snippet",
        DefaultParameterSetName = "ByObject"
    )]
    [OutputType(typeof(Object))]
    public class InvokeSnippetCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the snippet to invoke.",
            ParameterSetName = "ByName"
        )]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The snippet to invoke.",
            ParameterSetName = "ByObject"
        )]
        [Alias("Snippet")]
        [ValidateNotNullOrEmpty()]
        public Snippet InputObject { get; set; }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The parameters that will be passed into the snippet."
        )]
        [ValidateNotNullOrEmpty()]
        public Hashtable Parameters { get; set; }

        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the module that contains the snippet.",
            ParameterSetName = "ByName"
        )]
        [ValidateNotNullOrEmpty()]
        public string ModuleName { get; set; } = null;

        [Parameter(
            HelpMessage = "If true, invoke the snippet in a child scope; otherwise, invoke the snippet in the current scope."
        )]
        public SwitchParameter ChildScope { get; set; }

        protected override void ProcessRecord()
        {
            Snippet snippet = null;

            if (string.Compare(ParameterSetName, "ByName") == 0)
            {
                // If any of the Name parameter values contain a path delimiter (forward or backward slash), throw an exception
                if (Regex.IsMatch(Name, @"[\\/]"))
                {
                    throw new ParameterBindingException(@"Name cannot contain '\' or '/' characters.");
                }

                // If we have a ModuleName with wildcards, throw an exception
                if (MyInvocation.BoundParameters.ContainsKey("ModuleName") && WildcardPattern.ContainsWildcardCharacters(ModuleName))
                {
                    throw new ParameterBindingException("The ModuleName parameter cannot contain any wildcard characters.");
                }

                // If we have wildcards in the Name, throw an exception
                if (WildcardPattern.ContainsWildcardCharacters(Name))
                {
                    throw new ParameterBindingException("The Name parameter cannot contain any wildcard characters.");
                }

                // Create our snippet searcher object
                var snippetSearcher = new SnippetSearcher(MyInvocation.MyCommand.Module);

                // Look up the snippet being invoked
                snippet = snippetSearcher.FindItem(new SnippetSearchCriteria(Name, ModuleName, returnFirstItemFound: true, errorIfNotFound: true)).FirstOrDefault();
            }
            else
            {
                // Invoke the snippet that was passed in
                snippet = InputObject;
            }

            // Identify whether or not any mandatory parameters are missing
            List<string> missingMandatoryParameters = new List<string>();
            ScriptBlockAst ast = snippet.ScriptBlock.Ast as ScriptBlockAst;
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

            // If mandatory parameters are missing, throw a parameter binding exception
            if (missingMandatoryParameters.Count > 0)
            {
                throw new ParameterBindingException("The following mandatory parameters were not provided in the invocation of the \"" + Name + "\" snippet: " + String.Join(",", missingMandatoryParameters.ToArray()) + ".");
            }

            // Track the name of the snippet that is being invoked
            WriteVerbose(string.Format(@"Invoking snippet ""{0}"" in module ""{1}"".", snippet.Name, snippet.ModuleName));

            // Now invoke the snippet
            using (PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                ps.AddCommand(snippet.Path, MyInvocation.BoundParameters.ContainsKey("ChildScope") && ChildScope.IsPresent);
                if (MyInvocation.BoundParameters.ContainsKey("Parameters") && (Parameters.Count > 0))
                {
                    ps.AddParameters(Parameters);
                }
                Collection<PSObject> results = ps.Invoke();
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

            // Let the base class do its work
            base.ProcessRecord();
        }
    }
}