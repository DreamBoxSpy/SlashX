using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Services.Interfaces
{
    public interface IEventBus
    {
        public void Publish<TEvent>(object? sender, TEvent ev) where TEvent : EventArgs;
        public IDisposable Subscribe<TEvent>(EventHandler<TEvent> handler) where TEvent : EventArgs;
    }
}
