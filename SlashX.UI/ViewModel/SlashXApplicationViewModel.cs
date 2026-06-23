using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SlashX.Language;
using SlashX.UI.Event;
using SlashX.UI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SlashX.UI.ViewModel
{
    internal partial class SlashXApplicationViewModel(
        IServiceProvider service,
        SlashXApplicationModel? model
        ) : ViewModelBase
    {
        public SlashXApplicationViewModel(IServiceProvider service) : this(service, null) { }

        public record VMInitializedParameter(Action<string, string> SetAppResource,
                                        Action<Window> SetMainWindow);

        public void ApplyLanguage(Action<string, string> setAppResource)
        {
            var rm = Resource.ResourceManager;
            foreach (var v in rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true)!)
            {
                if (v is DictionaryEntry entry &&
                    entry.Value is string str)
                {
                    setAppResource("LOC_" + entry.Key, str);
                }
            }
        }

        [RelayCommand]
        public void Initialized(VMInitializedParameter parameter)
        {
            model?.OnCultureChanged(() => ApplyLanguage(parameter.SetAppResource));

            model?.PublishAppInitialized(this);

            {
                var mainWindow = new AppMainWindow()
                {
                    DataContext = service.GetRequiredService<AppMainWindowViewModel>()
                };
                parameter.SetMainWindow(mainWindow);

                model?.PublishMainWindowCreatedEvent(this);
            }

            ApplyLanguage(parameter.SetAppResource);
        }
    }
}
