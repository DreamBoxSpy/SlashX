using SlashX.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Services
{
    internal class EventBus : IEventBus
    {
        private class Listener<TEvent>(EventBus bus, EventHandler<TEvent> del) : IDisposable where TEvent : EventArgs
        {
            public void Dispose()
            {
                bus.Unsubscribe(del);
            }
        }

        private readonly ConcurrentDictionary<Type, List<Delegate>> handlers = [];

        public void Publish<TEvent>(object? sender, TEvent ev) where TEvent : EventArgs
        {
            if(handlers.TryGetValue(typeof(TEvent), out var list))
            {
                Delegate[] snapshot;
                lock(list)
                {
                    snapshot = [.. list];
                }
                foreach(var v in snapshot)
                {
                    ((EventHandler<TEvent>)v)(sender, ev);
                }
            }
        }

        public void Unsubscribe<TEvent>(EventHandler<TEvent> handler) where TEvent : EventArgs
        {
            var list = handlers.GetOrAdd(typeof(TEvent), _ => []);
            lock (list)
            {
                list.Remove(handler);
            }
        }

        public IDisposable Subscribe<TEvent>(EventHandler<TEvent> handler) where TEvent : EventArgs
        {
            var list = handlers.GetOrAdd(typeof(TEvent), _ => []);
            lock(list)
            {
                list.Add(handler);
                return new Listener<TEvent>(this, handler);
            }
        }
    }
}
