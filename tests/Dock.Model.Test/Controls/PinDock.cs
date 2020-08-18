
using System;

namespace Dock.Model.Controls
{
    public class PinDock : DockBase, IPinDock
    {
        public Alignment Alignment { get; set; } = Alignment.Unset;

        public bool IsExpanded { get; set; } = false;

        public bool AutoHide { get; set; } = true;

        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
