using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security;

namespace SnippetPx
{
    internal static class PowerShellExtensions
    {
        static private object DisablePromptToUpdateHelpLock = new object();

        static private bool DisablePromptToUpdateHelp(out int? oldValue)
        {
            oldValue = null;

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell"))
                {
                    oldValue = key?.GetValue("DisablePromptToUpdateHelp", null, RegistryValueOptions.None) as int?;
                    if (oldValue == null || (int)oldValue != 1)
                    {
                        key?.SetValue("DisablePromptToUpdateHelp", 1, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }

            return false;
        }

        static private bool ResetPromptToUpdateHelp(int? oldValue)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell"))
                {
                    if (oldValue != null)
                    {
                        key?.SetValue("DisablePromptToUpdateHelp", (int)oldValue, RegistryValueKind.DWord);
                    }
                    else
                    {
                        key?.DeleteValue("DisablePromptToUpdateHelp");
                    }

                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }

            return false;
        }

        static internal PSObject SafeGetExternalScriptHelp(ExternalScriptInfo externalScriptInfo)
        {
            lock (DisablePromptToUpdateHelpLock)
            {
                int? oldValue = null;
                bool resetRequired = false;

                try
                {
                    resetRequired = DisablePromptToUpdateHelp(out oldValue);

                    using (PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace))
                    {
                        ps.AddCommand("Microsoft.PowerShell.Core\\Get-Help");
                        ps.AddParameter("Name", externalScriptInfo.Path);
                        Collection<PSObject> results = ps.Invoke();
                        return results?[0];
                    }
                }
                finally
                {
                    if (resetRequired)
                    {
                        ResetPromptToUpdateHelp(oldValue);
                    }
                }
            }
        }
    }
}
