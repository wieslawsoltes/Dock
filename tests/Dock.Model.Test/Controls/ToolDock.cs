
using System;

namespace Dock.Model.Controls
{
    public class ToolDock : DockBase, IToolDock
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
