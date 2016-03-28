using System;

namespace SnippetPx
{
    [Serializable]
    public class SnippetSearchCriteria : ExternalScriptItemSearchCriteria
    {
        public SnippetSearchCriteria(string name, string moduleName = null, bool includeHelpInfo = false, bool returnFirstItemFound = false, bool errorIfNotFound = false)
            : base(name, moduleName, includeHelpInfo, returnFirstItemFound, errorIfNotFound)
        {
        }

        public override string SubfolderName { get { return "snippets"; } }
    }
}
