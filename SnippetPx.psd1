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

@{
      ModuleToProcess = 'SnippetPx.dll'

        ModuleVersion = '1.0.0.5'

                 GUID = '78755225-3595-445d-adfc-f59cf06f2fef'

               Author = 'Kirk Munro'

          CompanyName = 'Poshoholic Studios'

            Copyright = '© 2014 Kirk Munro'

          Description = 'The SnippetPx module enhances the snippet experience in PowerShell by offering a new format for Snippets: plain, ordinary ps1 files. These snippets are not just blocks of script that could be injected into a file. They are also invocable! This enables better reuse of commonly used pieces of script that would not otherwise be placed into a PowerShell function, either because the function support in PowerShell won''t allow for it to be invoked properly in the current scope, or because it isn''t big enough to warrant adding another function to the function pool.'

    PowerShellVersion = '3.0'

      CmdletsToExport = @(
                        'Get-Snippet'
                        'Invoke-Snippet'
                        )

             FileList = @(
                        'SnippetPx.psd1'
                        'SnippetPx.dll'
                        'en-us\SnippetPx.dll-Help.xml'
                        'license\gpl-3.0.txt'
                        'scripts\Install-SnippetPxModule.ps1'
                        'scripts\Uninstall-SnippetPxModule.ps1'
                        'snippets\Module.Initialize.ps1'
                        'snippets\ProxyFunction.Begin.ps1'
                        'snippets\ProxyFunction.End.ps1'
                        'snippets\ProxyFunction.Process.NoPipeline.ps1'
                        'snippets\ProxyFunction.Process.Pipeline.ps1'
                        'snippets\ScriptFile.Import.Ordered.ps1'
                        'snippets\ScriptFile.Import.ps1'
                        )

          PrivateData = @{
                            PSData = @{
                                Tags = 'snippet invoke scriptblock dry principle'
                                LicenseUri = 'http://www.gnu.org/licenses/gpl-3.0.html'
                                ProjectUri = 'https://github.com/KirkMunro/SnippetPx'
                                IconUri = ''
                                ReleaseNotes = ''
                            }
                        }
}