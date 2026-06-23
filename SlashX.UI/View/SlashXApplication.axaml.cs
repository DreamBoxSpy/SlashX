using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Browser;
using Dock.Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Language;
using SlashX.Language.Event;
using SlashX.Services.Interfaces;
using SlashX.UI.Event;
using SlashX.UI.Extensions;
using SlashX.UI.Model;
using SlashX.UI.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SlashX.UI.View
{
    public partial class SlashXApplication : Application
    {
        public SlashXApplication()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if(DataContext == null)
            {
                //Avalonia Designer
                var collection = new ServiceCollection();
                collection.AddViewModelServices();
                var service = collection.BuildServiceProvider();
                
                DataContext = service.GetRequiredService<SlashXApplicationViewModel>();
            }

            SlashXApplicationViewModel vm = (SlashXApplicationViewModel)DataContext;
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                vm.InitializedCommand.Execute(new(
                    SetAppResource: (key, val) => Resources[key] = val,
                    SetMainWindow: w => desktop.MainWindow = w
                ));
            }
       

            base.OnFrameworkInitializationCompleted();
        }

    }
}
