using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Services.Interfaces;
using SlashX.UI.Services.Interfaces;
using SlashX.UI.View;
using SlashX.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlashX.UI.Services
{
    internal class ApplicationEntry(
        IServiceProvider service
        ) : IApplicationDefault
    {

        // Avalonia Designer
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<SlashXApplication>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        public int Run()
        {
            var app = BuildAvaloniaApp()
                .AfterSetup(builder =>
                {
                    var app = (SlashXApplication?)builder.Instance;

                    Debug.Assert(app != null);

                    app.Service = service;

                    app.DataContext = new SlashXApplicationViewModel();
                });

            app.StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs(), Avalonia.Controls.ShutdownMode.OnMainWindowClose);
            return 0;
        }
        public static void Main()
        {

        }
    }
}
