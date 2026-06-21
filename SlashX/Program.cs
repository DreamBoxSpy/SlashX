using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.Versioning;
using System.Diagnostics;
using SlashX.Services;
using SlashX.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace SlashX
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var app = new CommandApp<AppCommand>();

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();

                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });

            services.Configure<SlashXConfiguration>(conf =>
            {
                var dataRootOverride = Environment.GetEnvironmentVariable("SLASHX_DATA_PATH");
                if(string.IsNullOrEmpty(dataRootOverride))
                {
                    dataRootOverride = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                }

                conf.DataRoot = dataRootOverride;
            });

            services.Configure<ModuleManager.Options>(opt =>
            {
                app.Configure(conf => opt.CommandLineConfigurator = conf);
                opt.ServiceCollections = services;
            });

            services.AddSingleton<IEventBus, EventBus>();

            services.AddSingleton<ILibraryManager, LibraryManager>();
            services.AddSingleton<IModuleManager, ModuleManager>();

            var provider = services.BuildServiceProvider();

            var moduleManager = provider.GetRequiredService<IModuleManager>();
            await moduleManager.LoadModules();

            app.WithData(services.BuildServiceProvider());

            return await app.RunAsync(args);
        }
    }
}
