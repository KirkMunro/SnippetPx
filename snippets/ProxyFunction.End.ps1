<#
.SYNOPSIS
    Proxy function end block
.DESCRIPTION
    The end block of a proxy function.
#>
[System.Diagnostics.DebuggerHidden()]
param()
try {
    #region Close the pipeline.

    $pipeline.End()

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}