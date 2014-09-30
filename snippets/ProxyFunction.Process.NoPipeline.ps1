<#
.SYNOPSIS
    Proxy function process block (no pipeline input)
.DESCRIPTION
    The process block of a proxy function that does not accept pipeline input.
#>
[System.Diagnostics.DebuggerHidden()]
param()
try {
    #region Invoke the process block without sending in pipeline input.

    $pipeline.Process()

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}