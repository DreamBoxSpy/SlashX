using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Core.Types;
using SkiaSharp;
using SlashX.UI.Model;
using SlashX.UI.ViewModel;
using SlashX.UI.ViewModel.Dock;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddModelServices(this IServiceCollection collection)
        {
            collection.AddSingleton<SlashXApplicationModel>();
            collection.AddSingleton<AppMainWindowModel>();
            collection.AddSingleton<AboutWindowModel>();
        }
        public static void AddViewModelServices(this IServiceCollection collection)
        {
            collection.AddSingleton<DockFactory>();
            collection.AddSingleton<IFactory, DockFactory>(service => service.GetRequiredService<DockFactory>());

            collection.AddSingleton<SlashXApplicationViewModel>();
            collection.AddSingleton<AppMainWindowViewModel>();

            collection.AddTransient<AboutWindowModel>();
        }
    }
}
