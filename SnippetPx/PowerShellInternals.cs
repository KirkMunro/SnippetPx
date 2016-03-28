using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace SnippetPx
{
    internal static class PowerShellInternals
    {
        static BindingFlags publicOrPrivateInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        static BindingFlags publicOrPrivateStatic   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        static internal Version GetPowerShellVersion()
        {
            // The PowerShell version is easy to access in PowerShell via the PSVersionTable variable;
            // however, to improve performance we need to be able to look up the version without using
            // PowerShell.  The internal Script.Management.Automation.PSVersionInfo type contains a
            // static GetPSVersionTable static method that gives us this important information.
            Assembly smaAssembly = typeof(PowerShell).Assembly;
            Type psVersionInfoType = smaAssembly.GetType("System.Management.Automation.PSVersionInfo");

            MethodInfo getPSVersionTableMethod = psVersionInfoType.GetMethod("GetPSVersionTable", publicOrPrivateStatic);

            return (getPSVersionTableMethod?.Invoke(null, publicOrPrivateStatic, null, new object[0], null) as Hashtable)?["PSVersion"] as Version;
        }

        internal static object GetExecutionContext(this Runspace runspace)
        {
            // Look up the Context (execution context) property value using the runspace
            return typeof(Runspace).GetProperty("ExecutionContext", publicOrPrivateInstance)
                                   ?.GetValue(runspace, null);

        }

        internal static EngineIntrinsics GetEngineIntrinsics(this Runspace runspace)
        {
            // Look up the Context (execution context) property value using the runspace
            object executionContext = runspace.GetExecutionContext();

            // Look up the ModuleIntrinsics instance using the executionContext
            return executionContext?.GetType()
                                   .GetProperty("EngineIntrinsics", publicOrPrivateInstance)
                                   ?.GetValue(executionContext, null) as EngineIntrinsics;
        }

        internal static string GetModuleBeingProcessed(this Runspace runspace)
        {
            // Look up the Context (execution context) property value using the runspace
            object executionContext = runspace.GetExecutionContext();

            // Look up the name of the module that is currently being processed (if any)
            return executionContext?.GetType()
                                   .GetProperty("ModuleBeingProcessed", publicOrPrivateInstance)
                                   ?.GetValue(executionContext, null) as string;
        }
        
        internal static object GetModuleIntrinsics(this Runspace runspace)
        {
            // Look up the Context (execution context) property value using the runspace
            object executionContext = runspace.GetExecutionContext();

            // Look up the ModuleIntrinsics instance using the executionContext
            return executionContext?.GetType()
                                   .GetProperty("Modules", publicOrPrivateInstance)
                                   ?.GetValue(executionContext, null);
        }

        internal static List<PSModuleInfo> GetLoadedModules(this Runspace runspace)
        {
            // Look up the ModuleIntrinsics instance using the psCmdlet
            object moduleIntrinsics = runspace.GetModuleIntrinsics();

            // Invoke the GetModules method on the ModuleIntrinsics instance
            return moduleIntrinsics?.GetType()
                                   .GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string[]), typeof(bool) }, null)
                                   ?.Invoke(moduleIntrinsics, new object[] { null, false }) as List<PSModuleInfo>;
        }

        internal static List<PSModuleInfo> GetLoadedModule(this Runspace runspace, string name, bool wildcardSearch = false)
        {
            // Look up the ModuleIntrinsics instance using the psCmdlet
            object moduleIntrinsics = runspace.GetModuleIntrinsics();

            // Invoke the GetExactMatchModules method on the ModuleIntrinsics instance
            return moduleIntrinsics?.GetType()
                                   .GetMethod("GetExactMatchModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(bool), typeof(bool) }, null)
                                   ?.Invoke(moduleIntrinsics, new object[] { name, false, !wildcardSearch }) as List<PSModuleInfo>;
        }

        internal static IEnumerable<CallStackFrame> GetCallStack(this Runspace runspace)
        {
            // The call stack is available via a GetCallStack method on the ScriptDebugger class;
            // however, since the ScriptDebugger class is not public, we cannot invoke its
            // GetCallStack method from within a binary module. This method removes that limitation
            // that shouldn't be there so that we can get the call stack via an API instead
            // of via a PowerShell cmdlet.
            Assembly smaAssembly = typeof(PowerShell).Assembly;
            Type scriptDebuggerType = smaAssembly.GetType("System.Management.Automation.ScriptDebugger");
            if ((scriptDebuggerType == null) || !scriptDebuggerType.IsInstanceOfType(runspace.Debugger))
            {
                return null;
            }

            var scriptDebugger = Convert.ChangeType(runspace.Debugger, scriptDebuggerType);
            return scriptDebuggerType.GetMethod("GetCallStack", publicOrPrivateInstance)
                                     ?.Invoke(scriptDebugger, new object[0]) as IEnumerable<CallStackFrame>;
        }
    }
}