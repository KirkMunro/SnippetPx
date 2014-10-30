<#############################################################################
The SnippetPx module enhances the snippet experience in PowerShell by offering
a new format for Snippets: plain, ordinary ps1 files. These snippets are not
just blocks of script that could be injected into a file. They are also
invocable! This enables better reuse of commonly used pieces of script that
would not otherwise be placed into a PowerShell function, either because the
function support in PowerShell won't allow for it to be invoked properly in
the current scope, or because it isn't big enough to warrant adding another
function to the function pool.

Copyright 2014 Kirk Munro

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
#############################################################################>

@{
      ModuleToProcess = 'SnippetPx.dll'

        ModuleVersion = '1.0.0.7'

                 GUID = '78755225-3595-445d-adfc-f59cf06f2fef'

               Author = 'Kirk Munro'

          CompanyName = 'Poshoholic Studios'

            Copyright = 'Copyright 2014 Kirk Munro'

          Description = 'The SnippetPx module enhances the snippet experience in PowerShell by offering a new format for Snippets: plain, ordinary ps1 files. These snippets are not just blocks of script that could be injected into a file. They are also invocable! This enables better reuse of commonly used pieces of script that would not otherwise be placed into a PowerShell function, either because the function support in PowerShell won''t allow for it to be invoked properly in the current scope, or because it isn''t big enough to warrant adding another function to the function pool.'

    PowerShellVersion = '3.0'

      CmdletsToExport = @(
                        'Get-Snippet'
                        'Invoke-Snippet'
                        )

             FileList = @(
                        'LICENSE'
                        'NOTICE'
                        'SnippetPx.psd1'
                        'SnippetPx.dll'
                        'en-us\SnippetPx.dll-Help.xml'
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
                                LicenseUri = 'http://apache.org/licenses/LICENSE-2.0.txt'
                                ProjectUri = 'https://github.com/KirkMunro/SnippetPx'
                                IconUri = ''
                                ReleaseNotes = ''
                            }
                        }
}