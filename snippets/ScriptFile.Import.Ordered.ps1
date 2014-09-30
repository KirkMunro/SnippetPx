<#
.SYNOPSIS
    Import script files (ordered)
.DESCRIPTION
    Import one or more script files in a specific order into the current scope.
#>
[System.Diagnostics.DebuggerHidden()]
param(
    # The path where the script files are located.
    [System.String]
    $Path,

    # The order in which you want the script files loaded. Any discovered files not included in this order will be imported after the ordered files.
    [System.String[]]
    $Order,

    # A filter to select which script files should be imported.
    [System.String]
    $Filter = '*.ps1'
)
#region Import each script file matching our filter that is found in the path specified, using a custom sort order.

Get-ChildItem -Path $Path -Filter $Filter `
    | Sort-Object -Property @(
        @{
            Expression = {
                if (($index = $Order.IndexOf($_.Name)) -ge 0) {
                    $index
                } else {
                    $Order.Count
                }
            }
        }
        'Name'
    ) `
    | ForEach-Object {
        . $_.FullName
    }

#endregion