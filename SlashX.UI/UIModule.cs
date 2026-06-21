using Microsoft.Extensions.DependencyInjection;
using SlashX.Abstractions;
using SlashX.Services.Interfaces;
using SlashX.UI.Services;
using SlashX.UI.Services.Interfaces;

namespace SlashX.UI
{
    internal class UIModule : IModule
    {
        public void Initialize(IModuleInitializeContext context)
        {
            context.Services.AddSingleton<IApplicationDefault, ApplicationDefault>();
            context.Services.AddSingleton<ISlashXApplication, ApplicationDefault>(s => (ApplicationDefault) s.GetRequiredService<IApplicationDefault>());
        }
    }
}
