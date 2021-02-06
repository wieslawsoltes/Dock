using System.Text;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Docks
{
    public class FilesDocumentDock : DocumentDock
    {
        public FilesDocumentDock()
        {
            CreateDocument = ReactiveCommand.Create(CreateNewDocument);
        }

        private void CreateNewDocument()
        {
            if (!CanCreateDocument)
            {
                return;
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
        }
    }
}
