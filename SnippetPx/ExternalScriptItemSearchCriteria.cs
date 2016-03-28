using System;
using System.Management.Automation;

namespace SnippetPx
{
    [Serializable]
    abstract public class ExternalScriptItemSearchCriteria : DiscoverableItemSearchCriteria
    {
        public ExternalScriptItemSearchCriteria(string name, string moduleName = null, bool includeHelpInfo = false, bool returnFirstItemFound = false, bool errorIfNotFound = false)
            : base(name, moduleName, returnFirstItemFound, errorIfNotFound)
        {
            IncludeHelpInfo = includeHelpInfo;
        }

        public bool IncludeHelpInfo { get; private set; }

        public override string FileExtension { get { return "ps1"; } }

        public override CommandTypes CommandType { get { return CommandTypes.ExternalScript; } }
    }
}
