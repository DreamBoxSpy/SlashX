using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Abstractions
{
    public interface IModule
    {
        public void Initialize(IModuleInitializeContext context);
    }
    public interface IModuleInitializeContext
    {
        IServiceCollection ServiceCollection { get; }
        IConfigurator CommandLineConfigurator { get; }
    }
}
