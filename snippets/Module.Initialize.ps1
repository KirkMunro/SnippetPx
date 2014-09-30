<#
.SYNOPSIS
    Initialize script module
.DESCRIPTION
    Initialize a script module with strict mode, explicit export, and variables that should be set by default.
#>
[System.Diagnostics.DebuggerHidden()]
param(
    # The strict mode version that will be applied to the module.
    [System.String]
    $StrictModeVersion = 'Latest'
)
#region Initialize the module.

# Set strict mode so that PowerShell helps avoid errors in the module.
Set-StrictMode -Version $StrictModeVersion
# Enable explicit export so that there are no surprises with commands exported from the module.
Export-ModuleMember
# Define PSModule and PSModuleRoot if they are not defined already (this approach is forward-compatible).
if (-not (Get-Variable -Name PSModule -Scope 0 -ErrorAction Ignore)) {
    $PSModule = $ExecutionContext.SessionState.Module
}
if (-not (Get-Variable -Name PSModuleRoot -Scope 0 -ErrorAction Ignore)) {
    $PSModuleRoot = $PSModule.ModuleBase
}

#endregion