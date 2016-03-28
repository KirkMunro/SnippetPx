using System.Management.Automation;
using System.Linq;
using System.Text.RegularExpressions;

namespace SnippetPx
{
    [Cmdlet(
        VerbsCommon.Get,
        "Snippet"
    )]
    [OutputType(typeof(Snippet))]
    public class GetSnippetCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            HelpMessage = "The name of a snippet."
        )]
        [ValidateNotNullOrEmpty()]
        [SupportsWildcards()]
        public string[] Name { get; set; } = new string[] { "*" };

        [Parameter(
            Position = 1,
            HelpMessage = "The name of the module that contains the snippet."
        )]
        [ValidateNotNullOrEmpty()]
        [SupportsWildcards()]
        public string ModuleName { get; set; }

        [Parameter()]
        public SwitchParameter NoHelp = false;

        protected override void BeginProcessing()
        {
            // If any of the Name parameter values contain a path delimiter (forward or backward slash), throw an exception
            if (Name.Any(x => Regex.IsMatch(x, @"[\\/]")))
            {
                throw new ParameterBindingException(@"Name cannot contain '\' or '/' characters.");
            }

            // Let the base class do its work
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // Create our snippet searcher object
            var snippetSearcher = new SnippetSearcher(MyInvocation.MyCommand.Module);

            // Output any snippets matching the search criteria
            foreach (var snippet in Name.SelectMany(x => snippetSearcher.FindItem(new SnippetSearchCriteria(x, ModuleName, includeHelpInfo: NoHelp == false, errorIfNotFound : !WildcardPattern.ContainsWildcardCharacters(x) && !WildcardPattern.ContainsWildcardCharacters(ModuleName)))))
            {
                WriteObject(snippet);
            }

            // Let the base class do its work
            base.ProcessRecord();
        }
    }
}