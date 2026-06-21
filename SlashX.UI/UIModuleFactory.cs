using Microsoft.Extensions.DependencyInjection;
using SlashX.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI
{
    internal class UIModuleFactory : IModuleFactory
    {
        int IModuleFactory.Priority => 20000;
        public IModule Create(IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance<UIModule>(serviceProvider);
        }
    }
}
