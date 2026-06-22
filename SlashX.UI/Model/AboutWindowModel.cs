using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SlashX.UI.Model
{
    internal partial class AboutWindowModel : ObservableObject
    {

        public string AppVersion { get; }

        public AboutWindowModel()
        {
            var fullVer = typeof(SlashX.Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if(!string.IsNullOrEmpty(fullVer))
            {
                var splitIdx = fullVer.IndexOf('+');
                var ver = fullVer[..splitIdx];
                var commit = fullVer[(splitIdx + 1)..];

                fullVer = $"{ver}+{commit[..7]}";
            }
            else
            {
                fullVer = typeof(SlashX.Program).Assembly.GetName().Version!.ToString();
            }

            AppVersion = "v" + fullVer;
        }
    }
}
