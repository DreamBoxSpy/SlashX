using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Browser;
using Dock.Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Language;
using SlashX.Language.Event;
using SlashX.Services.Interfaces;
using SlashX.UI.Event;
using SlashX.UI.Model;
using SlashX.UI.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SlashX.UI.View
{
    internal class SlashXApplication(
        ISlashXApplication app,
        IEventBus bus,
        IServiceProvider service
        ) : Application
    {
        private void ApplyLanguage()
        {
            var rm = Resource.ResourceManager;
            foreach(var v in rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true)!)
            {
                if(v is DictionaryEntry entry &&
                    entry.Value is string str)
                {
                    Resources["LOC_" + entry.Key] = str;
                }
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            app?.Application = this;

            ApplyLanguage();

            bus?.Subscribe<CurrentCultureChangedEvent>((_, _1) =>
            {
                ApplyLanguage();
            });

            Styles.Add(new DockFluentTheme());
            Styles.Add(new FluentTheme());

            Resources.MergedDictionaries.Add(new ResourceInclude((Uri?)null)
            {
                Source = new Uri("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml")
            });
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new AppMainWindow()
                {
                    Bus = bus
                };

                if(service != null)
                {
                    desktop.MainWindow.DataContext = ActivatorUtilities.CreateInstance<AppMainWindowModel>(service);
                }
            }
       
            bus?.Publish<SlashXApplicationCreated>(this, new()
            {
                Application = this
            }); 
            
            base.OnFrameworkInitializationCompleted();
        }

    }
}
