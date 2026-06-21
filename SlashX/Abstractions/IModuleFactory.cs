using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Abstractions
{
    public interface IModuleFactory
    {
        public IModule Create(IServiceProvider serviceProvider);
        public int Priority => 100000;
    }
}
