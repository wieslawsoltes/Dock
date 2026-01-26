using DockOfficeSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Documents;

public class WordDocumentView : ReactiveUserControl<WordDocumentViewModel>
{
    public WordDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
