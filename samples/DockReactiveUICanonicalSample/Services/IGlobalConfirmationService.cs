using System;
using System.ComponentModel;

namespace DockReactiveUICanonicalSample.Services;

public interface IGlobalConfirmationService : INotifyPropertyChanged
{
    bool IsConfirmationOpen { get; }

    string? Message { get; }

    IDisposable Begin(string? message = null);
}
