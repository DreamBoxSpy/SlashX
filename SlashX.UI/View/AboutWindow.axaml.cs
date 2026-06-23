using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SlashX.UI.Model;
using SlashX.UI.ViewModel;
using System.Diagnostics;
using System.Reflection;

namespace SlashX.UI;

public partial class AboutWindow : Window
{
   
    public AboutWindow()
    {
        InitializeComponent();

        DataContext = new AboutWindowViewModel();
    }

    private async void OnViewRepositoryClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "https://github.com/DreamBoxSpy/SlashX",
            UseShellExecute = true
        });
    }
}