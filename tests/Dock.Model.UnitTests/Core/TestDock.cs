
using System;

namespace Dock.Model.UnitTests
{
    public class TestDock : DockBase
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
