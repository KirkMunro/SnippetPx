<#############################################################################
The SnippetPx module enhances the snippet experience in PowerShell by offering
a new format for Snippets: plain, ordinary ps1 files. These snippets are not
just blocks of script that could be injected into a file. They are also
invocable! This enables better reuse of commonly used pieces of script that
would not otherwise be placed into a PowerShell function, either because the
function support in PowerShell won't allow for it to be invoked properly in
the current scope, or because it isn't big enough to warrant adding another
function to the function pool.

Copyright © 2014 Kirk Munro.

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License in the
license folder that is included in the SnippetPx module. If not, see
<https://www.gnu.org/licenses/gpl.html>.
#############################################################################>

# This script should only be invoked when you want to uninstall SnippetPx.

[CmdletBinding(SupportsShouldProcess=$true)]
[OutputType([System.Void])]
param(
    [Parameter()]
    [System.Management.Automation.SwitchParameter]
    $RemovePersistentData
)
try {
    #region Get the currently installed module (if there is one).

    Write-Progress -Activity 'Uninstalling SnippetPx' -Status 'Looking for an installed SnippetPx module.'
    $module = Get-Module -ListAvailable | Where-Object {$_.Guid -eq [System.Guid]'78755225-3595-445d-adfc-f59cf06f2fef'}
    if ($module -is [System.Array]) {
        [System.String]$message = 'More than one version of SnippetPx is installed on this system. This is not supported. Manually remove the versions you are not using and then try again.'
        [System.Management.Automation.SessionStateException]$exception = New-Object -TypeName System.Management.Automation.SessionStateException -ArgumentList $message
        [System.Management.Automation.ErrorRecord]$errorRecord = New-Object -TypeName System.Management.Automation.ErrorRecord -ArgumentList $exception,'SessionStateException',([System.Management.Automation.ErrorCategory]::InvalidOperation),$module
        throw $errorRecord
    }

    #endregion

    #region Remove the module.

    if ($module) {
        Write-Progress -Activity 'Uninstalling SnippetPx' -Status 'Unloading and removing the installed SnippetPx module.'
        # Unload the module if it is currently loaded.
        if ($loadedModule = Get-Module | Where-Object {$_.Guid -eq $module.Guid}) {
            $loadedModule | Remove-Module -ErrorAction Stop
        }
        # Remove the currently installed module.
        Remove-Item -LiteralPath $module.ModuleBase -Recurse -Force -ErrorAction Stop
    }

    #endregion

    #region Now remove the persistent data for the module if the caller requested it.

    if ($PSCmdlet.MyInvocation.BoundParameters.ContainsKey('RemovePersistentData') -and $RemovePersistentData) {
        foreach ($mlsRoot in $env:LocalAppData,$env:ProgramData) {
            $mlsPath = Join-Path -Path $mlsRoot -ChildPath "WindowsPowerShell\Modules\$($module.Name)"
            if (Test-Path -LiteralPath $mlsPath) {
                Remove-Item -LiteralPath $mlsPath -Recurse -Force -ErrorAction Stop
            }
        }
    }

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}