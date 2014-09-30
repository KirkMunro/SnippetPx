<#
.SYNOPSIS
    Proxy function begin block
.DESCRIPTION
    The begin block of a proxy function.
#>
[System.Diagnostics.DebuggerStepThrough()]
param(
    # The name of the command being proxied.
    [System.String]
    $CommandName,

    # The type of the command being proxied. Valid values include 'Cmdlet' or 'Function'.
    [System.Management.Automation.CommandTypes]
    $CommandType,

    # A script block that is used to convert from the proxy command parameters to the parameters of the command being proxied.
    [System.Management.Automation.ScriptBlock]
    $PreProcessScriptBlock = {}
)
try {
    #region Ensure that objects are sent through the pipeline one at a time.

    $outBuffer = $null
    if ($PSCmdlet.MyInvocation.BoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer)) {
        $PSCmdlet.MyInvocation.BoundParameters['OutBuffer'] = 1
    }

    #endregion

    #region Look up the command being proxied.

    $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand($CommandName, $CommandType)

    #endregion

    #region If the command was not found, throw an appropriate command not found exception.

    if (-not $wrappedCmd) {
        $PSCmdlet.ThrowCommandNotFoundError($CommandName, $PSCmdlet.MyInvocation.MyCommand.Name)
    }

    #endregion

    . $PreProcessScriptBlock

    #region Create the proxy command script block.

    $PSPassThruParameters = $PSCmdlet.MyInvocation.BoundParameters
    $scriptCmd = {& $wrappedCmd @PSPassThruParameters}

    #endregion

    #region Use the script block to create the pipeline, then invoke its begin block.

    $pipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
    $pipeline.Begin($PSCmdlet)

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}