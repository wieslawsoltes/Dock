using DockOfficeSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Documents;

public class WordDocumentView : ReactiveUserControl<WordDocumentViewModel>
{
    public WordDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
