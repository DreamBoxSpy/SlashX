using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Event
{
    public class SlashXApplicationCreatedEvent : EventArgs
    {
        public required Application Application { get; init; }
    }
}
