using Avalonia;
using CommunityToolkit.Mvvm.Input;
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
    internal partial class SlashXApplicationViewModel : ViewModelBase<SlashXApplicationModel>
    {
        public void ApplyLanguage(Application app)
        {
            var rm = Resource.ResourceManager;
            foreach (var v in rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true)!)
            {
                if (v is DictionaryEntry entry &&
                    entry.Value is string str)
                {
                    app.Resources["LOC_" + entry.Key] = str;
                }
            }
        }

        [RelayCommand]
        public void Initialized(Application app)
        {
            Model?.OnCultureChanged(() => ApplyLanguage(app));

            Model?.PublishAppInitialized(app, new()
            {
                Application = app
            });

            ApplyLanguage(app);
        }
    }
}
