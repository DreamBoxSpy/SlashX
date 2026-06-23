using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using SlashX.Language;
using SlashX.UI.Model;
using SlashX.UI.ViewModel.Dock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SlashX.UI.ViewModel
{
    internal partial class AppMainWindowViewModel : ViewModelBase
    {
        public DockFactory DockFactory { get; set; }
        public IRootDock DockLayout { get; set; }

        public AppMainWindowViewModel(
            DockFactory dockFactory,
            AppMainWindowModel? model
        )
        {
            DockFactory = dockFactory;
            DockLayout = dockFactory.CreateLayout()!;
            DockFactory.InitLayout(DockLayout);
        }

        public AppMainWindowViewModel(DockFactory dockFactory) : this(dockFactory, null) { }

    }
}
