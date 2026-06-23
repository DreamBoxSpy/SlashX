using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Services.Interfaces
{
    public interface IServiceProviderApplication
    {
        public IServiceProvider? Service { get; }
    }
}
