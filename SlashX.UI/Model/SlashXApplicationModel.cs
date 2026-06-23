using SlashX.Language.Event;
using SlashX.Services.Interfaces;
using SlashX.UI.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Model
{
    internal class SlashXApplicationModel(
        IEventBus bus
        )
    {
        public void PublishMainWindowCreatedEvent(object? sender)
        {
            bus.Publish<MainWindowCreatedEvent>(sender, new());
        }
        public void OnCultureChanged(Action callback)
        {
            bus.Subscribe<CurrentCultureChangedEvent>((_, _1) => callback());
        }
        public void PublishAppInitialized(object? sender)
        {
            bus.Publish<SlashXApplicationCreatedEvent>(sender, new());
        }
    }
}
