using Dock.Model;

namespace AvaloniaDemo.ViewModels
{
    public interface IMainWindowViewModel
    {
        IDockFactory Factory { get; set; }
        IView Layout { get; set; }
        string CurrentView { get; set; }
    }
}
