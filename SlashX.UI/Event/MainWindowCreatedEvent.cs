using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Event
{
    public class MainWindowCreatedEvent : EventArgs
    {
        public required Window MainWindow { get; init; }
    }
}
