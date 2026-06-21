using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Services.Interfaces;
using SlashX.UI.Services.Interfaces;
using SlashX.UI.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlashX.UI.Services
{
    internal class ApplicationDefault(
        IServiceProvider service
        ) : IApplicationDefault, ISlashXApplication
    {
        public Application? Application { get; set; }

        // Avalonia Designer
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(() => new SlashXApplication(null!, null!, null!))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        private AppBuilder BuildApp() => AppBuilder.Configure(() => 
            ActivatorUtilities.CreateInstance<SlashXApplication>(service))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
        public int Run()
        {
            var app = BuildApp();
            app.StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs(), Avalonia.Controls.ShutdownMode.OnMainWindowClose);
            return 0;
        }
        public static void Main()
        {

        }
    }
}
