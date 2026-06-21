using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Event
{
    public class SlashXApplicationCreated : EventArgs
    {
        public required Application Application { get; init; }
    }
}
