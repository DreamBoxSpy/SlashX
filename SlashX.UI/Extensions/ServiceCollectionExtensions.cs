using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Core.Types;
using SkiaSharp;
using SlashX.UI.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddViewModelServices(this IServiceCollection collection)
        {

            collection.AddSingleton<AppMainWindowModel>();
            collection.AddSingleton<AboutWindowModel>();
        }
    }
}
