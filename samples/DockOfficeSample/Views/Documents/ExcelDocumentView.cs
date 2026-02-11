using DockOfficeSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockOfficeSample.Views.Documents;

public class ExcelDocumentView : ReactiveUserControl<ExcelDocumentViewModel>
{
    public ExcelDocumentView()
    {
        Content = new OfficeDocumentView();
    }
}
