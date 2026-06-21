using SlashX.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Services.Interfaces
{
    public interface IModuleManager
    {

        public IModuleGroup CreateModuleGroup(string id);
        public void RemoveModuleGroup(string id);
        public IModuleGroup? GetModuleGroup(string id);
        public IEnumerable<IModuleGroup> GetModuleGroups();


        public IReadOnlyList<IModule> LoadedModules { get; }
        internal Task LoadModules();
    }
}
