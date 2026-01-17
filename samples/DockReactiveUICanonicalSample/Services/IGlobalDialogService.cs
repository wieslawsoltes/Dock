using System;
using System.ComponentModel;

namespace DockReactiveUICanonicalSample.Services;

public interface IGlobalDialogService : INotifyPropertyChanged
{
    bool IsDialogOpen { get; }

    string? Message { get; }

    IDisposable Begin(string? message = null);
}
