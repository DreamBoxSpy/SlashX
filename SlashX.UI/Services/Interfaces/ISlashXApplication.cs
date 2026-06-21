using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Services.Interfaces
{
    public interface ISlashXApplication
    {
        public Application? Application { get; internal set; }
    }
}
