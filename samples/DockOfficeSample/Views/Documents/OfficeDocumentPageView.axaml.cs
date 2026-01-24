using Avalonia.Markup.Xaml;
using DockOfficeSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockOfficeSample.Views.Documents;

public partial class OfficeDocumentPageView : ReactiveUserControl<OfficeDocumentPageViewModel>
{
    public OfficeDocumentPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
