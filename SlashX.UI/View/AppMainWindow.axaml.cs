using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using SlashX.Language;
using SlashX.Services.Interfaces;
using SlashX.UI.Event;
using SlashX.UI.Extensions;
using SlashX.UI.Model;
using SlashX.UI.ViewModel;
using SlashX.UI.ViewModel.Dock;
using System.Diagnostics;

namespace SlashX.UI;

public partial class AppMainWindow : Window
{
    public AppMainWindow()
    {
        InitializeComponent();
    }

    private void OnFileMenuExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    private async void OnAboutAvaloniaClick(object? sender, RoutedEventArgs e)
    {
        await new AboutAvaloniaDialog().ShowDialog(this);
    }
    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        await new AboutWindow().ShowDialog(this);
    }
}