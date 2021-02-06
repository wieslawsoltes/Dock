using AvaloniaDemo.ViewModels.Documents;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Controls;

namespace AvaloniaDemo.ViewModels.Docks
{
    public class CustomDocumentDock : DocumentDock
    {
        public override IDocument? CreateDocument()
        {
            if (!CanCreateDocument)
            {
                return base.CreateDocument();
            }

            var index = VisibleDockables?.Count + 1;
            var document = new DocumentViewModel {Id = $"Document{index}", Title = $"Document{index}"};

            Factory?.AddDockable(this, document);
            Factory?.SetActiveDockable(document);
            Factory?.SetFocusedDockable(this, document);

            return document;
        }
    }
}
