using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Abstractions
{
    public interface IModuleGroup
    {
        public string Id { get; }
        public bool IsEnabled { get; set; }
        public IDictionary<string, string> Modules { get; }
        public void Save();
    }
}
