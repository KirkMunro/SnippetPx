using System;
using System.Management.Automation;

namespace SnippetPx
{
    [Serializable]
    abstract public class DiscoverableItem<TCommandInfo> where TCommandInfo : CommandInfo
    {
        protected TCommandInfo info;
        public string Name { get; private set; }
        public string ModuleName { get; private set; } = null;
        abstract public string Path { get; }
        public int Rank { get; internal set; }
        internal DiscoverableItem(TCommandInfo commandInfo, string moduleName = null)
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(commandInfo.Name);
            info = commandInfo;
            ModuleName = moduleName;
        }
    }
}
