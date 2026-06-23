using Dock.Model;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Abstractions;
using SlashX.Services.Interfaces;
using SlashX.UI.Extensions;
using SlashX.UI.Model;
using SlashX.UI.Services;

namespace SlashX.UI
{
    internal class UIModule : IModule
    {
        public void Initialize(IModuleInitializeContext context)
        {
            context.ServiceCollection.AddSingleton<IDockManager, DockManager>();
            context.ServiceCollection.AddSingleton<IApplicationDefault, ApplicationEntry>();

            context.ServiceCollection.AddModelServices();
            context.ServiceCollection.AddViewModelServices();
        }
    }
}
