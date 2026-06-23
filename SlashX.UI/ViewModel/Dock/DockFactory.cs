using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.UI.ViewModel.Dock
{
    internal class DockFactory : Factory
    {
        public DocumentDock? Documents { get; set; }
        public override IRootDock CreateLayout()
        {
            var root = CreateRootDock();

            Documents = new()
            {
                Id = "DocumentDockRoot",
                CanCreateDocument = false,
                IsCollapsable = false,
                VisibleDockables = CreateList<IDockable>()
            };

            root.VisibleDockables = CreateList<IDockable>([
                Documents
                ]);
            
            return root;
        }
    }
}
