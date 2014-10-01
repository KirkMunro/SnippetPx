using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace SnippetPx
{
    public class Snippet
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Synopsis { get; set; }
        public string Description { get; set; }
        public ScriptBlock ScriptBlock { get; set; }
        public Snippet(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
