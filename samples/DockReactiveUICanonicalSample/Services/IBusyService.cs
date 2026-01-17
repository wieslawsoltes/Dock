using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DockReactiveUICanonicalSample.Services;

public interface IBusyService : INotifyPropertyChanged
{
    bool IsBusy { get; }

    string? Message { get; }

    bool IsReloadVisible { get; set; }

    bool CanReload { get; }

    ICommand ReloadCommand { get; }

    IDisposable Begin(string message);

    Task RunAsync(string message, Func<Task> action);

    void UpdateMessage(string? message);

    void SetReloadHandler(Func<Task>? handler);
}
