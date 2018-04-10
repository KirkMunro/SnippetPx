<#
.SYNOPSIS
    Create a new alias and export it from the module
.DESCRIPTION
    Create a new alias that is associated with a cmdlet or function and export the alias from the module in which it is created. If the alias is optional and provided for convenience only, no error will be raised if the alias cannot be created. If the alias is required and cannot be created (because the command name is already in use), an error will be raised.
#>
[System.Diagnostics.DebuggerHidden()]
param(
    # The name of the alias.
    [System.String[]]
    $Name,

    # The command that the alias is associated with.
    [System.String]
    $Value,

    # When set, indicates that the alias is required by the module and that the module will not load if the alias cannot be created.
    [System.Management.Automation.SwitchParameter]
    $Required = $false
)
#region Create and export an alias.

foreach ($aliasName in $Name) {
    New-Alias -Name $aliasName -Value $Value -Force -ErrorAction Ignore
    if ($?) {
        Export-ModuleMember -Alias $aliasName
    } elseif ($Required) {
        $message = "Failed to create alias ${aliasName}. An alias by the name ${aliasName} already exists. Module $($PSModule.Name) requires this alias to function properly. It will not load until this conflict has been removed."
        $exception = New-Object -TypeName System.Management.Automation.SessionStateException -ArgumentList $message
        $errorRecord = New-Object -TypeName System.Management.Automation.ErrorRecord -ArgumentList @(
            $exception
            $exception.GetType().Name
            [System.Management.Automation.ErrorCategory]::ResourceExists
            Get-Alias -Name $aliasName
        )
        throw $errorRecord
    }
}

#endregion