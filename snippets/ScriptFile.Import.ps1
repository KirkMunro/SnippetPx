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

& {
    $callerSignature = $null
    if (($callerStackEntry = Get-PSCallStack | Where-Object {$_.Command -eq 'Invoke-Snippet'} | Select-Object -First 1) -and
        (-not [System.String]::IsNullOrWhitespace($callerStackEntry.ScriptName))) {
        $callerSignature = Get-AuthenticodeSignature -FilePath $callerStackEntry.ScriptName
    }
    Get-ChildItem -Path $Path -Filter $Filter `
        | Where-Object {
            ($_.Name -like $Filter) -and
            (($callerSignature -eq $null) -or
             ($callerSignature.Status -eq [System.Management.Automation.SignatureStatus]::NotSigned) -or
             (($callerSignature.Status -eq [System.Management.Automation.SignatureStatus]::Valid) -and
              ($fileSignature = Get-AuthenticodeSignature -FilePath $_.FullName) -and
              ($fileSignature.Status -eq [System.Management.Automation.SignatureStatus]::Valid) -and
              ($fileSignature.SignerCertificate -eq $callerSignature.SignerCertificate)))
        }
} | ForEach-Object {
    . $_.FullName
}

#endregion