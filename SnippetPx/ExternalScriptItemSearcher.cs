using System.Management.Automation;

namespace SnippetPx
{
    abstract public class ExternalScriptItemSearcher<TOutput, TSearchCriteria> : DiscoverableItemSearcher<TOutput, TSearchCriteria, ExternalScriptInfo>
        where TOutput : ExternalScriptItem
        where TSearchCriteria : ExternalScriptItemSearchCriteria
    {
        public ExternalScriptItemSearcher(PSModuleInfo invokedFromModule = null)
            : base(invokedFromModule)
        {
        }
    }
}
