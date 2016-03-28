using System;
using System.Linq;
using System.Management.Automation;

namespace SnippetPx
{
    [Serializable]
    public class Snippet : ExternalScriptItem
    {
        public string Synopsis { get; internal set; } = null;
        public string Description { get; internal set; } = null;
        public Snippet(ExternalScriptInfo snippetInfo, string moduleName = null, PSObject helpInfo = null)
            : base(snippetInfo, moduleName)
        {
            if (helpInfo != null)
            {
                Synopsis = helpInfo.Properties["Synopsis"]?.Value as string;
                var descriptionPropertyValue = (helpInfo.Properties["Description"]?.Value as PSObject[])?.Select(x => x.Properties["Text"]?.Value).ToArray();
                if (descriptionPropertyValue != null)
                {
                    Description = string.Join("\r\n", descriptionPropertyValue);
                }
            }
        }
    }
}
