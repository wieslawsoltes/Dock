
using System;

namespace Dock.Model.Controls
{
    public class ToolDock : DockBase, IToolDock
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
