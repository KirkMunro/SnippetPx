using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SnippetPx
{
    internal static class PowerShellInternals
    {
        internal static List<PSModuleInfo> GetLoadedModules(this PSCmdlet psCmdlet)
        {
            // Look up the Context internal property
            PropertyInfo contextProperty = typeof(PSCmdlet).GetProperty("Context", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (contextProperty != null)
            {
                object executionContext = contextProperty.GetValue(psCmdlet, null);
                if (executionContext != null)
                {
                    PropertyInfo modulesProperty = executionContext.GetType().GetProperty("Modules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (modulesProperty != null)
                    {
                        object modules = modulesProperty.GetValue(executionContext, null);
                        if (modules != null)
                        {
                            MethodInfo getModulesMethod = modules.GetType().GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string[]), typeof(bool) }, null);
                            if (getModulesMethod != null)
                            {
                                return (List<PSModuleInfo>)getModulesMethod.Invoke(modules, new object[] { null, false });
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}