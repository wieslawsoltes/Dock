using System.Text;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Documents;

namespace Notepad.ViewModels.Docks
{
    public class FilesDocumentDock : DocumentDock
    {
        public override IDocument? CreateDocument()
        {
            if (!CanCreateDocument)
            {
                return base.CreateDocument();
            }

            var document = new FileViewModel()
            {
                Path = string.Empty,
                Title = "Untitled",
                Text = "",
                Encoding = Encoding.Default.WebName
            };

            Factory?.AddDockable(this, document);
            Factory?.SetActiveDockable(document);
            Factory?.SetFocusedDockable(this, document);

            return document;
        }
    }
}
