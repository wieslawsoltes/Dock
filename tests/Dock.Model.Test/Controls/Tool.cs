
namespace Dock.Model.Controls
{
    public class Tool : DockableBase, ITool, IDocument
    {
        public override IDockable? Clone()
        {
            return this;
        }
    }
}
