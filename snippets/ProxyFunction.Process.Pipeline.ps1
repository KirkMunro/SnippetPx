<#
.SYNOPSIS
    Proxy function process block
.DESCRIPTION
    The process block of a proxy function.
#>
[System.Diagnostics.DebuggerHidden()]
param()
try {
    #region Process the element that was just received from the previous stage in the pipeline.

    $pipeline.Process($_)

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}