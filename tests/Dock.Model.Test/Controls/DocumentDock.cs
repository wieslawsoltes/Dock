
using System;

namespace Dock.Model.Controls
{
    public class DocumentDock : DockBase, IDocumentDock
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
