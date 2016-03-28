using System.Management.Automation;

namespace SnippetPx
{
    public class SnippetSearcher : ExternalScriptItemSearcher<Snippet, SnippetSearchCriteria>
    {
        public SnippetSearcher(PSModuleInfo invokedFromModule = null)
            : base(invokedFromModule)
        {
        }

        protected override Snippet NewItemInstance(ExternalScriptInfo externalScriptInfo, SnippetSearchCriteria searchCriteria, string foundInModule)
        {
            return new Snippet(
                externalScriptInfo,
                foundInModule,
                helpInfo: searchCriteria.IncludeHelpInfo ? PowerShellExtensions.SafeGetExternalScriptHelp(externalScriptInfo) : null);
        }
    }
}
