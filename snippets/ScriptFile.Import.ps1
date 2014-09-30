<#
.SYNOPSIS
    Import script files
.DESCRIPTION
    Import one or more script files into the current scope.
#>
[System.Diagnostics.DebuggerHidden()]
param(
    # The path where the script files are located.
    [System.String]
    $Path,

    # A filter to select which script files should be imported.
    [System.String]
    $Filter = '*.ps1'
)
#region Import each script file matching our filter that is found in the path specified.

Get-ChildItem -Path $Path -Filter $Filter | ForEach-Object {
    . $_.FullName
}

#endregion