## SnippetPx

### Overview

The SnippetPx module enhances the snippet experience in PowerShell by offering
a new format for Snippets: plain, ordinary ps1 files. These snippets are not
just blocks of script that could be injected into a file. They are also
invocable! This enables better reuse of commonly used pieces of script that
would not otherwise be placed into a PowerShell function, either because the
function support in PowerShell won't allow for it to be invoked properly in
the current scope, or because it isn't big enough to warrant adding another
function to the function pool.

Snippets are automatically discovered based on their inclusion in the following
folders (in the order in which they are listed):
- the current user snippets folder (Documents\WindowsPowerShell\snippets);
- the all users snippets folder (Program Files\WindowsPowerShell\snippets);
- the snippets folder in the SnippetPx module (SnippetPx\snippets);
- the snippets folder in all other modules, in the order in which they are
discovered according to the PSModulePath environment variable;

If multiple snippets with the same name exist in different folders, only the
first snippet with that name will be discovered. To guarantee uniqueness of
snippets across modules, snippets specific to a module should use a snippet name
prefixed with the module name. For example, a snippet to provision AD users in an
ActiveDirectory module could use the filename ActiveDirectory.User.Provision.ps1.

Using spaces in snippet filenames is supported, but discouraged.

Not all snippets are appropriate for use in all situations. When in doubt, consult
the documentation for the snippet to ensure it is appropriate for your use case.

### Minimum requirements

- PowerShell 3.0

### License and Copyright

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

### Using the SnippetPx module

To see a list of all snippets that are available in your environment, invoke
the following command:

```powershell
Get-Snippet
```

This will return a list of all snippets that have been discovered on the
local system in the snippets folders that it found. Each snippet object will
include the name of the snippet, the path to the snippet file, a synopsis
identifying what the snippet does, a description that describes the snippet
in a little more detail, and a script block that contains the body of the
snippet.

Once you have identified a snippet that you want to invoke in one of your
scripts, you can invoke that snippet with a command like the following:

```powershell
# Import all ps1 files in the functions folder into the current scope
Invoke-Snippet -Name ScriptFile.Import -Parameters @{
    Path = Join-Path -Path $PSModulePath -ChildPath functions
}
```

Some snippets are included with the SnippetsPx module, and others may be
discovered in other modules or in the current user or all user snippets
folders. It is important to note that invoking a snippet alone does not
automatically load the module in which it is contained. The module will
only be auto-loaded if the snippet itself contains a command that would
trigger the auto-loading of the module. This is worth understanding for
modules that include snippets that work with module content other than
discoverable commands.

### Command List

The SnippetPx module currently includes the following commands:

```powershell
Get-Snippet
Invoke-Snippet
```