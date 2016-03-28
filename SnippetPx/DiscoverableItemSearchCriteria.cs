using System;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace SnippetPx
{
    [Serializable]
    abstract public class DiscoverableItemSearchCriteria
    {
        public DiscoverableItemSearchCriteria(string name, string moduleName = null, bool returnFirstItemFound = false, bool errorIfNotFound = false)
        {
            if (Regex.IsMatch(name, @"[\\/]"))
            {
                throw new ArgumentException(@"The ""name"" argument cannot contain forward slash (""/"") or backward slash (""\"") characters.", name);
            }

            Name = name;
            ModuleName = moduleName;
            ReturnFirstItemFound = returnFirstItemFound;
            ErrorIfNotFound = errorIfNotFound;
            IsWildcardInName = WildcardPattern.ContainsWildcardCharacters(name);
            IsWildcardInModuleName = WildcardPattern.ContainsWildcardCharacters(moduleName);
        }

        public string Name { get; private set; }

        public string ModuleName { get; private set; }

        public abstract string SubfolderName { get; }

        public abstract string FileExtension { get; }

        public virtual CommandTypes CommandType { get { return CommandTypes.Application; } }

        public bool ReturnFirstItemFound { get; private set; }

        public bool ErrorIfNotFound { get; private set; }

        public bool IsWildcardInName { get; private set; }

        public bool IsWildcardInModuleName { get; private set; }
    }
}
