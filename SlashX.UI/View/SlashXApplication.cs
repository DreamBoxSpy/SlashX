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
using SlashX.UI.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SlashX.UI.View
{
    internal class SlashXApplication : Application, IServiceProviderApplication
    {
        public IServiceProvider? Service { get; internal set; }


        private void InitializeStyles()
        {
            Styles.Add(new DockFluentTheme());
            Styles.Add(new FluentTheme());
            Resources.MergedDictionaries.Add(new ResourceInclude((Uri?)null)
            {
                Source = new Uri("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml")
            });
        }
        public override void OnFrameworkInitializationCompleted()
        {
            InitializeStyles();

            Debug.Assert(DataContext != null);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new AppMainWindow
                {
                    DataContext = new AppMainWindowViewModel()
                };

            }
       
            var vm = (SlashXApplicationViewModel)DataContext;
            vm.InitializedCommand.Execute(this);

            base.OnFrameworkInitializationCompleted();
        }

    }
}
