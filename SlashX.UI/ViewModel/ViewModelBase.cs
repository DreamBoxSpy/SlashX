using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SlashX.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.ViewModel
{
    public partial class ViewModelBase : ObservableObject
    {
        
    }
    public partial class ViewModelBase<TModel> : ViewModelBase
    {
        public TModel? Model { get; }

        public ViewModelBase()
        {
            var app = Application.Current;
            if(app is IServiceProviderApplication provider &&
                provider.Service is IServiceProvider service)
            {
                Model = service.GetService<TModel>();
            }
        }
    }
}
