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

namespace SlashX.UI;

public partial class AppMainWindow : Window
{
    public IEventBus? Bus { get; set; }

    public DocumentDock DocumentDock { get; set; }

    protected override void OnInitialized()
    {
        Bus?.Publish<MainWindowCreatedEvent>(this, new()
        {
            MainWindow = this
        });
        base.OnInitialized();
    }
    public AppMainWindow()
    {
        InitializeComponent();

        var factory = new Factory();
        var root = factory.CreateRootDock();

        DocumentDock = new DocumentDock
        {
            Id = "Documents",
            IsCollapsable = false,
            CanCreateDocument = false,
            VisibleDockables = factory.CreateList<IDockable>([
                new Document(){
                    Title = Resource.DOCUMENT_TITLE_WELCOME
                }
                ])
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(
                   DocumentDock
                   )
        };


        root.VisibleDockables = factory.CreateList<IDockable>([
            mainLayout
            ]); 

        factory.InitLayout(root);

        DockController.Factory = factory;
        DockController.Layout = root;
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