using System;
using System.Management.Automation;

namespace SnippetPx
{
    [Serializable]
    public class ExternalScriptItem : DiscoverableItem<ExternalScriptInfo>
    {
        public override string Path { get { return info.Path; } }
        public ScriptBlock ScriptBlock { get { return info.ScriptBlock; } }
        public ExternalScriptItem(ExternalScriptInfo externalScriptInfo, string moduleName = null)
            : base(externalScriptInfo, moduleName)
        {
        }
    }
}
