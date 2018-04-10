using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace SnippetPx
{
    abstract public class DiscoverableItemSearcher<TOutput, TSearchCriteria, TSearchResult>
        where TOutput         : DiscoverableItem<TSearchResult>
        where TSearchCriteria : DiscoverableItemSearchCriteria
        where TSearchResult   : CommandInfo
    {
        public DiscoverableItemSearcher(PSModuleInfo invokedFromModule = null)
        {
            InvokedFromModule = invokedFromModule;
        }

        private int rank = -1;
        private List<string> pathsSearched = new List<string>();
        private bool peekAtResults = false;

        protected PSModuleInfo InvokedFromModule { get; private set; }

        protected abstract TOutput NewItemInstance(TSearchResult commandInfo, TSearchCriteria searchCriteria, string foundInModule);

        private TOutput CreateNewItemInstance(TSearchResult commandInfo, TSearchCriteria searchCriteria, string foundInModule)
        {
            var result = NewItemInstance(commandInfo, searchCriteria, foundInModule);
            result.Rank = rank;
            return result;
        }

        public IEnumerable<TOutput> FindItem(TSearchCriteria searchCriteria)
        {
            // Reset the rank and the paths searched since we're doing a new search
            rank = -1;
            pathsSearched.Clear();

            // Search for the discoverable item
            var results = FindDiscoverableItem(searchCriteria);

            // After search efforts have been exhausted, if the user asked for an exception indicating that the item
            // being searched for was not found, give them one
            if (searchCriteria.ErrorIfNotFound && !results.Any())
            {
                string message = null;
                if (!string.IsNullOrEmpty(searchCriteria.ModuleName) && !string.IsNullOrEmpty(searchCriteria.SubfolderName))
                {
                    message = string.Format(
                        @"No item matching ""{0}.{1}"" was found in the ""{2}"" subfolder of the ""{3}"" module.",
                        searchCriteria.Name,
                        searchCriteria.FileExtension,
                        searchCriteria.SubfolderName,
                        searchCriteria.ModuleName
                    );
                }
                else if (string.IsNullOrEmpty(searchCriteria.SubfolderName))
                {
                    message = string.Format(
                        @"No item matching ""{0}.{1}"" was found in the root folder of the ""{2}"" module.",
                        searchCriteria.Name,
                        searchCriteria.FileExtension,
                        searchCriteria.ModuleName
                    );
                }
                else if (string.IsNullOrEmpty(searchCriteria.ModuleName))
                {
                    message = string.Format(
                        @"No item matching ""{0}.{1}"" was found in the ""{2}"" subfolder of a module or a WindowsPowerShell profile folder.",
                        searchCriteria.Name,
                        searchCriteria.FileExtension,
                        searchCriteria.SubfolderName
                    );
                }
                else
                {
                    message = string.Format(
                        @"No item matching ""{0}.{1}"" was found in the root folder of a module or a WindowsPowerShell profile folder.",
                        searchCriteria.Name,
                        searchCriteria.FileExtension
                    );
                }
                throw new DiscoverableItemNotFoundException(
                    searchCriteria.Name,
                    message
                );
            }

            return searchCriteria.ReturnFirstItemFound ? results.Take(1) : results.Distinct().OrderBy(x => x.Rank).ThenBy(x => x.Name).ThenBy(x => x.ModuleName).ToList();
        }

        protected List<TOutput> FindDiscoverableItem(TSearchCriteria searchCriteria)
        {
            var results = new List<TOutput>();
            var currentCount = 0;

            // If we have a module-qualified name, return the discoverable item(s) from that module
            if (!string.IsNullOrEmpty(searchCriteria.ModuleName))
            {
                rank++;
                results.AddRange(FindInLoadingModule(searchCriteria));
                if ((searchCriteria.ReturnFirstItemFound || !searchCriteria.IsWildcardInModuleName) && results.Count > 0)
                {
                    return results;
                }

                if (results.Count > currentCount)
                {
                    rank++;
                    currentCount = results.Count;
                }
                results.AddRange(FindInLoadedModule(searchCriteria));
                if ((searchCriteria.ReturnFirstItemFound || !searchCriteria.IsWildcardInModuleName) && results.Count > 0)
                {
                    return results;
                }

                if (results.Count > currentCount)
                {
                    rank++;
                    currentCount = results.Count;
                }
                results.AddRange(FindInUnloadedModule(searchCriteria));

                return results;
            }

            // If the discoverable item was not module qualified, we need to look more broadly

            // Where we look depends on the version of PowerShell that is running
            string currentUserPowerShellBaseFolder;
            string allUsersPowerShellBaseFolder;
            string powerShellSubfolder;

            // PowerShell Core on Linux or macOS
            if (PowerShellInternals.IsUnixPlatform())
            {
                currentUserPowerShellBaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                allUsersPowerShellBaseFolder = "/usr/local/share";
                powerShellSubfolder = "powershell";
            }
            // PowerShell Core on Windows or Windows PowerShell
            else
            {
                currentUserPowerShellBaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
                allUsersPowerShellBaseFolder = Environment.GetEnvironmentVariable("ProgramFiles");
                powerShellSubfolder = PowerShellInternals.IsPowerShellCore() ? "PowerShell" : "WindowsPowerShell";
            }

            // Check the current user WindowsPowerShell folder
            rank++;
            results.AddRange(FindInPath(Path.Combine(currentUserPowerShellBaseFolder, powerShellSubfolder), searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check the Program Files\WindowsPowerShell folder
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInPath(Path.Combine(allUsersPowerShellBaseFolder, powerShellSubfolder), searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check the loading module folder (if a module is loading)
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInLoadingModule(searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check the caller's module folder (if the caller is in a module)
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInCallingModule(searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check the current module folder
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInCurrentModule(searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check all other loaded module folders (being careful to check for discoverable item
            // ambiguity among loaded modules)
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInLoadedModule(searchCriteria).ToList());
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Check all unloaded module folders (being careful to check for discoverable item
            // ambiguity among each PSModulePath segment)
            if (results.Count > currentCount)
            {
                rank++;
                currentCount = results.Count;
            }
            results.AddRange(FindInUnloadedModule(searchCriteria));
            if (searchCriteria.ReturnFirstItemFound && results.Count > 0)
            {
                return results;
            }

            // Return the collection of discoverable items that we found
            return results;
        }

        protected IEnumerable<TOutput> FindInLoadingModule(TSearchCriteria searchCriteria)
        {
            // Lookup the module that is currently loading
            string loadingModule = Runspace.DefaultRunspace.GetModuleBeingProcessed();
            if (string.IsNullOrEmpty(loadingModule))
            {
                return new List<TOutput>();
            }

            // If we are looking for a specific module and the module currently loading
            // is not the module we are looking for, stop looking at the module that is
            // currently loading
            string loadingModuleName = Path.GetFileNameWithoutExtension(loadingModule);
            if (!string.IsNullOrEmpty(searchCriteria.ModuleName) &&
                !searchCriteria.IsWildcardInModuleName && 
                string.Compare(searchCriteria.ModuleName, loadingModuleName, true) != 0)
            {
                return new List<TOutput>();
            }

            // If the module that is currently loading has the subfolder we are looking for,
            // return any matching snippets in that module's subfolder path
            string loadingModuleBase = Path.GetDirectoryName(loadingModule);
            if (string.IsNullOrEmpty(searchCriteria.SubfolderName) ||
                Directory.Exists(Path.Combine(loadingModuleBase, searchCriteria.SubfolderName)))
            {
                return FindInPath(loadingModuleBase, searchCriteria, loadingModuleName);
            }

            // If we made it this far, the currently loading module does not have the subfolder
            // containing the discoverable items we are looking for
            return new List<TOutput>();
        }

        protected IEnumerable<TOutput> FindInLoadedModule(TSearchCriteria searchCriteria)
        {
            var results = from loadedModule in string.IsNullOrEmpty(searchCriteria.ModuleName) ? Runspace.DefaultRunspace.GetLoadedModules()
                                                                                               : Runspace.DefaultRunspace.GetLoadedModule(searchCriteria.ModuleName, searchCriteria.IsWildcardInModuleName)
                          where IsSubfolderInModulePath(loadedModule.ModuleBase, loadedModule.Name, searchCriteria.SubfolderName)
                          from discoverableItem in FindInPath(loadedModule.ModuleBase, searchCriteria, loadedModule.Name)
                          select discoverableItem;

            AssertNameNotAmbiguous(results, searchCriteria);

            return results;
        }

        protected IEnumerable<TOutput> FindInUnloadedModule(TSearchCriteria searchCriteria)
        {
            return from moduleLookupFolder in Environment.GetEnvironmentVariable("PSModulePath").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                   where Directory.Exists(moduleLookupFolder)
                   from discoverableItem in FindInUnloadedModuleLookupFolder(moduleLookupFolder, searchCriteria)
                   select discoverableItem;
        }

        protected IEnumerable<TOutput> FindInUnloadedModuleLookupFolder(string unloadedModuleLookupFolder, TSearchCriteria searchCriteria)
        {
            var results = from discoverableItemFolder in string.IsNullOrEmpty(searchCriteria.ModuleName) ? Directory.EnumerateDirectories(unloadedModuleLookupFolder)
                                                                                                         : Directory.EnumerateDirectories(unloadedModuleLookupFolder, searchCriteria.ModuleName, SearchOption.TopDirectoryOnly)
                          from directory in ExpandUnloadedModuleDiscoverableItemFolder(discoverableItemFolder, searchCriteria)
                          from discoverableItem in FindInPath(directory, searchCriteria, Path.GetFileName(discoverableItemFolder))
                          select discoverableItem;

            AssertNameNotAmbiguous(results, searchCriteria);

            return results;
        }

        protected IEnumerable<string> ExpandUnloadedModuleDiscoverableItemFolder(string discoverableItemFolder, TSearchCriteria searchCriteria)
        {
            string moduleName = Path.GetFileName(discoverableItemFolder);
            Version powerShellVersion = PowerShellInternals.GetPowerShellVersion();
            if (powerShellVersion > new Version("5.0"))
            {
                var versionedSubfolders = from subDirectory in Directory.EnumerateDirectories(discoverableItemFolder)
                                          let version = ConvertToVersion(Path.GetFileName(subDirectory))
                                          where version != null && IsSubfolderInModulePath(subDirectory, moduleName, searchCriteria.SubfolderName)
                                          orderby version descending
                                          select subDirectory;
                if (versionedSubfolders.Any())
                {
                    return versionedSubfolders;
                }
            }

            if (IsSubfolderInModulePath(discoverableItemFolder, moduleName, searchCriteria.SubfolderName))
            {
                var results = new List<string>();
                results.Add(discoverableItemFolder);
                return results;
            }
            
            return new List<string>();
        }

        protected IEnumerable<TOutput> FindInPath(string path, TSearchCriteria searchCriteria, string foundInModule = null)
        {
            // Modify the path and name so that they can be used to find discoverable items
            path = path.TrimEnd(new char[] { '\\' });
            if (!string.IsNullOrEmpty(searchCriteria.SubfolderName) &&
                String.Compare(Path.GetFileName(path), searchCriteria.SubfolderName, true) != 0)
            {
                path = Path.Combine(path, searchCriteria.SubfolderName);
            }
            string name = string.Format("{0}.{1}", searchCriteria.Name, searchCriteria.FileExtension);

            // If we're doing a wildcard search on a non-existent path, return an empty list
            if (searchCriteria.IsWildcardInName && !Directory.Exists(path))
            {
                return new List<TOutput>();
            }

            // If we've already searched this path, return an empty list
            if (pathsSearched.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                return new List<TOutput>();
            }

            // Add the current path to the list of paths searched if we're not simply peeking at the result set
            if (!peekAtResults)
            {
                pathsSearched.Add(path);
            }

            // Verify that we can get the EngineIntrinsics instance for command lookup
            var executionContext = Runspace.DefaultRunspace.GetEngineIntrinsics();
            if (executionContext == null)
            {
                throw new PropertyNotFoundException(string.Format(@"Property ""{0}"" not found on the default runspace instance.", "EngineIntrinsics"));
            }

            // Build a query string that processes discovered items matching the path and name
            try
            {
                return from file in Directory.GetFiles(path, name, SearchOption.TopDirectoryOnly)
                       let commandInfo = executionContext.InvokeCommand.GetCommand(file, searchCriteria.CommandType) as TSearchResult
                       where commandInfo != null
                       select CreateNewItemInstance(commandInfo, searchCriteria, foundInModule);
            }
            catch (DirectoryNotFoundException)
            {
                // If the directory is not found, simply swallow the exception and return an empty list
            }

            return new List<TOutput>();
        }

        protected IEnumerable<TOutput> FindInCallingModule(TSearchCriteria searchCriteria)
        {
            string modulePath = null;
            string moduleName = null;
            List<PSModuleInfo> loadedModules = null;
            var callStack = Runspace.DefaultRunspace.GetCallStack()?.ToList();
            for (int index = 0; index < callStack?.Count; index++)
            {
                // Get the current call stack frame
                var callStackFrame = callStack?[index];

                // Resolve the command that was invoked in that call stack frame, making
                // sure to resolve aliases if there are aliases to resolve
                var commandInfo = callStackFrame?.InvocationInfo.MyCommand;
                if (commandInfo is AliasInfo)
                {
                    commandInfo = (commandInfo as AliasInfo).ResolvedCommand;
                }

                // If the command has a module associated with it, harvest the module path,
                // but only if it is different than that of the current module
                if (commandInfo?.Module != null && InvokedFromModule != null)
                {
                    if (string.Compare(commandInfo.Module.ModuleBase, InvokedFromModule.ModuleBase, true) != 0)
                    {
                        modulePath = commandInfo.Module.ModuleBase;
                        moduleName = commandInfo.Module.Name;
                    }
                }
                // Otherwise if the command contains a script block that is defined within
                // a module folder that contains discoverable items, harvest that module path
                else
                {
                    // Pull the script block out of the command
                    ScriptBlock scriptBlock = null;
                    if (commandInfo is FunctionInfo)
                    {
                        scriptBlock = (commandInfo as FunctionInfo).ScriptBlock;
                    }
                    else if (commandInfo is WorkflowInfo)
                    {
                        scriptBlock = (commandInfo as WorkflowInfo).ScriptBlock;
                    }
                    else if (commandInfo is ScriptInfo)
                    {
                        scriptBlock = (commandInfo as ScriptInfo).ScriptBlock;
                    }
                    else if (commandInfo is ExternalScriptInfo)
                    {
                        scriptBlock = (commandInfo as ExternalScriptInfo).ScriptBlock;
                    }

                    // If the script block is defined in a file in a module folder, harvest
                    // that module path
                    var tempPath = scriptBlock?.File;
                    while (!string.IsNullOrEmpty(tempPath))
                    {
                        tempPath = Path.GetDirectoryName(tempPath);
                        if (loadedModules == null)
                        {
                            loadedModules = Runspace.DefaultRunspace.GetLoadedModules();
                        }
                        var loadedModule = loadedModules.Where(x => string.Compare(x.ModuleBase, tempPath, true) == 0).FirstOrDefault();
                        if (loadedModule != null)
                        {
                            modulePath = loadedModule.ModuleBase;
                            moduleName = loadedModule.Name;
                            break;
                        }
                    }
                }

                // If no module path was found, continue processing the call stack
                if (string.IsNullOrEmpty(modulePath))
                {
                    continue;
                }

                // Return the query to search for the discoverable item in the module that was found
                return FindInPath(modulePath, searchCriteria, moduleName);
            }

            // Return an empty list when no modules were found in the call stack
            return new List<TOutput>();
        }

        protected IEnumerable<TOutput> FindInCurrentModule(TSearchCriteria searchCriteria)
        {
            return InvokedFromModule != null ? FindInPath(InvokedFromModule.ModuleBase, searchCriteria, InvokedFromModule.Name) : null;
        }

        private bool IsSubfolderInModulePath(string path, string moduleName, string subfolderName)
        {
            return (string.IsNullOrEmpty(subfolderName) ||
                    Directory.Exists(Path.Combine(path, subfolderName))) &&
                   (File.Exists(Path.Combine(path, moduleName + ".psd1")) ||
                    File.Exists(Path.Combine(path, moduleName + ".psm1")) ||
                    File.Exists(Path.Combine(path, moduleName + ".dll")));
        }

        private Version ConvertToVersion(string versionString)
        {
            Version version;
            if (Version.TryParse(versionString, out version))
            {
                return version;
            }
            return null;
        }

        private void AssertNameNotAmbiguous(IEnumerable<TOutput> possibleMatches, TSearchCriteria searchCriteria)
        {
            if (searchCriteria.ReturnFirstItemFound &&
                (string.IsNullOrEmpty(searchCriteria.ModuleName) || searchCriteria.IsWildcardInModuleName))
            {
                peekAtResults = true;
                try
                {
                    if (possibleMatches.Take(2).ToList().Count == 2)
                    {
                        string message = null;
                        if (!string.IsNullOrEmpty(searchCriteria.SubfolderName))
                        {
                            message = string.Format(
                                @"The ""{0}.{1}"" name does not resolve to a single discoverable item of type {2} in the ""{3}"" folder of a module. Include a module name to disambiguate discoverable items.",
                                searchCriteria.Name,
                                searchCriteria.FileExtension,
                                searchCriteria.CommandType,
                                searchCriteria.SubfolderName
                            );
                        }
                        else
                        {
                            message = string.Format(
                                @"The ""{0}.{1}"" name does not resolve to a single discoverable item of type {2} in the root folder of a module. Include a module name to disambiguate discoverable items.",
                                searchCriteria.Name,
                                searchCriteria.FileExtension,
                                searchCriteria.CommandType
                            );
                        }
                        throw new DiscoverableItemNameAmbiguousException<TOutput>(
                            searchCriteria.Name,
                            possibleMatches,
                            message
                        );
                    }
                }
                finally
                {
                    peekAtResults = false;
                }
            }
        }

    }
}
