using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlashX.Abstractions;
using SlashX.Services.Interfaces;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlashX.Services
{
    internal class ModuleManager(
        IServiceProvider servicesProvider,
        ILogger<ModuleManager> logger,
        IOptions<ModuleManager.Options> options,
        IOptions<SlashXConfiguration> conf,
        ILibraryManager libraryManager
        ) : IModuleManager
    {
        private class ModuleInitializeContext(
            Options options
            ) : IModuleInitializeContext
        {
            public IServiceCollection Services => options.ServiceCollections;

            public IConfigurator CommandLineConfigurator => options.CommandLineConfigurator;
        }
        internal class Options
        {
            public IServiceCollection ServiceCollections { get; set; } = null!;
            public IConfigurator CommandLineConfigurator { get; set; } = null!;
        }

        private class ModuleGroup : IModuleGroup
        {
            [JsonInclude]
            internal Dictionary<string, string> modules = [];

            [JsonIgnore]
            internal string? savePath;

            [JsonIgnore]
            public IDictionary<string, string> Modules => modules;

            [JsonIgnore]
            public string Id { get; set; } = "";

            [JsonInclude]
            public bool IsEnabled { get; set; } = true;

            public void Save()
            {
                if(string.IsNullOrEmpty(savePath))
                {
                    return;
                }
                
                File.WriteAllText(savePath, JsonSerializer.Serialize(this));
            }
        }

        private readonly string MODULE_GROUPS_ROOT = Path.Combine(conf.Value.DataRoot, "modules");
        private readonly List<IModule> modules = [];
        private readonly Dictionary<string, IModuleGroup> groups = [];

        public IReadOnlyList<IModule> LoadedModules => modules;

        private async Task LoadModuleGroups()
        {
            Directory.CreateDirectory(MODULE_GROUPS_ROOT);

            groups.Clear();

            foreach(var v in Directory.EnumerateFiles(MODULE_GROUPS_ROOT, "*.json", SearchOption.TopDirectoryOnly))
            {
                var id = Path.GetFileNameWithoutExtension(v);
                var group = JsonSerializer.Deserialize<ModuleGroup>(await File.ReadAllTextAsync(v));
                if(group == null)
                {
                    continue;
                }
                group.Id = id;
                group.savePath = v;

                groups.Add(id, group);
            }
        }

        async Task IModuleManager.LoadModules()
        {
            await LoadModuleGroups();

            Dictionary<string, string> modulePackages = [];

            foreach(var group in GetModuleGroups())
            {
                logger.LogTrace("Found module group: {group} (Enabled: {enabled})", group.Id, group.IsEnabled);

                if(!group.IsEnabled)
                {
                    continue;
                }
                foreach((var packageId, var packageVersion) in group.Modules)
                {
                    await libraryManager.FixDependencies(packageId, packageVersion);
                    modulePackages.Add(packageId, packageVersion);
                }
            }

            logger.LogTrace("Resolving module packages");

            var libraries = await libraryManager.ResolveLibraries(default, 
                modulePackages.Select(x => $"{x.Key}-{x.Value}") 
                );

            List<Assembly> assemblies = [];

            foreach(var v in libraries)
            {
                if(v.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        assemblies.Add(Assembly.LoadFrom(v));
                    }
                    catch (BadImageFormatException) when (v.EndsWith(".dll"))
                    {
                        //Native Library?
                        NativeLibrary.Load(v);
                    }
                }
                else
                {
                    //Native Library
                    NativeLibrary.Load(v);
                }
            }

            logger.LogTrace("Finding module factories");

            List<IModuleFactory> factories = [];
            foreach(var v in assemblies)
            {
                foreach (var type in v.GetTypes())
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }
                    if (!type.IsAssignableTo(typeof(IModuleFactory)))
                    {
                        continue;
                    }
                    var factory = (IModuleFactory) ActivatorUtilities.CreateInstance(servicesProvider, type);
                    factories.Add(factory);
                }
            }

            factories.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            logger.LogTrace("Building modules"); 
            
            var moduleContext = new ModuleInitializeContext(options.Value);

            foreach (var v in factories)
            {
                var module = v.Create(servicesProvider);
                modules.Add(module);

                module.Initialize(moduleContext);
            }
        }

        public IModuleGroup CreateModuleGroup(string id)
        {
            var group = new ModuleGroup()
            {
                Id = id,
                savePath = Path.Combine(MODULE_GROUPS_ROOT, id + ".json")
            };
            groups.Add(id, group);
            return group;
        }

        public void RemoveModuleGroup(string id)
        {
            groups.Remove(id);
        }

        public IModuleGroup? GetModuleGroup(string id)
        {
            if(groups.TryGetValue(id, out var result))
            {
                return result;
            }
            return null;
        }

        public IEnumerable<IModuleGroup> GetModuleGroups()
        {
            return groups.Values;
        }
    }
}
