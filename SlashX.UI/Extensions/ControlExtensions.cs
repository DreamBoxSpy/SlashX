using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.Extensions
{
    public static class ControlExtensions
    {
        public static TViewModel GetViewModel<TViewModel>(this StyledElement control)
        {
            return (TViewModel)(control.DataContext ?? throw new NullReferenceException());
        }
    }
}
