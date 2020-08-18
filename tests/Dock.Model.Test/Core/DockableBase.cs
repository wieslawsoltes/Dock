
namespace Dock.Model
{
    public abstract class DockableBase : IDockable
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public object? Context { get; set; }

        public IDockable? Owner { get; set; }

        public IFactory? Factory { get; set; }

        public virtual bool OnClose()
        {
            return true;
        }

        public virtual void OnSelected()
        {
        }

        public abstract IDockable? Clone();
    }
}
