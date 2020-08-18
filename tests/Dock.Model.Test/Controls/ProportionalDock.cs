
using System;

namespace Dock.Model.Controls
{
    public class ProportionalDock : DockBase, IProportionalDock
    {
        public Orientation Orientation { get; set; }

        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
