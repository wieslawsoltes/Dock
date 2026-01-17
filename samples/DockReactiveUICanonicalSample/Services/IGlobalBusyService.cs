using System;
using System.ComponentModel;

namespace DockReactiveUICanonicalSample.Services;

public interface IGlobalBusyService : INotifyPropertyChanged
{
    bool IsBusy { get; }

    string? Message { get; }

    IDisposable Begin(string? message = null);
}
