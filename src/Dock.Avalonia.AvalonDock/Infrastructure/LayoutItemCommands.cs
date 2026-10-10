using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Windows.Input;
using Avalonia;
using AvalonDock.Controls;
using ReactiveUI.Reactive;

namespace AvalonDock.Infrastructure;

// Owns only the defaults. Application commands and their bindings are never
// disposed or overwritten when a container is detached or restyled.
internal sealed class LayoutItemCommands(LayoutItem owner) : IDisposable
{
    private readonly List<Entry> _entries = new(13);

    public void Add(StyledProperty<ICommand?> property, Action execute, Func<bool> canExecute)
        => _entries.Add(new Entry(property, execute, canExecute));

    public void Apply()
    {
        foreach (Entry entry in _entries)
            if (owner.GetValue(entry.Property) is null)
                owner.SetCurrentValue(entry.Property, entry.Command);
    }

    public void Clear()
    {
        foreach (Entry entry in _entries)
            if (ReferenceEquals(owner.GetValue(entry.Property), entry.Command))
                owner.SetCurrentValue(entry.Property, null);
    }

    public void Refresh()
    {
        foreach (Entry entry in _entries) entry.Refresh();
    }

    public void Dispose()
    {
        foreach (Entry entry in _entries) entry.Dispose();
        _entries.Clear();
    }

    private sealed class Entry : IDisposable
    {
        private readonly Func<bool> _canExecute;
        private readonly BehaviorSubject<bool> _enabled;
        private bool _disposed;

        public Entry(StyledProperty<ICommand?> property, Action execute, Func<bool> canExecute)
        {
            Property = property;
            _canExecute = canExecute;
            _enabled = new(canExecute());
            Command = ReactiveCommand.Create(() =>
            {
                // Also guard direct ICommand.Execute calls and state changes
                // occurring while an application handles another command.
                if (!_disposed && _canExecute()) execute();
            }, _enabled, ImmediateScheduler.Instance);
        }

        public StyledProperty<ICommand?> Property { get; }
        public ReactiveCommand<Unit, Unit> Command { get; }

        public void Refresh()
        {
            if (_disposed) return;
            bool enabled = _canExecute();
            if (_enabled.Value != enabled) _enabled.OnNext(enabled);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _enabled.OnNext(false);
            Command.Dispose();
            _enabled.Dispose();
        }
    }
}
