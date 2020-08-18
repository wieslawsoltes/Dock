
namespace Dock.Model.Controls
{
    public class Document : DockableBase, IDocument
    {
        public override IDockable? Clone()
        {
            return this;
        }
    }
}
