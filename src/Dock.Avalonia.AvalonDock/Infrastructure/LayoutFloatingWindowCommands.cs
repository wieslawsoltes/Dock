using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows.Input;
using Avalonia;
using AvalonDock.Controls;
using AvalonDock.Layout;
using ReactiveUI.Reactive;

namespace AvalonDock.Infrastructure;

// Own window commands and their subscriptions, never the application commands.
internal sealed class LayoutFloatingWindowCommands : IDisposable
{
    private ILayoutElement? _model;
    private Func<ILayoutElement?>? _getModel;
    private LayoutRoot? _root;
    private readonly List<LayoutItem> _items = new();
    private readonly HashSet<ICommand> _observed = new();
    private readonly Command _close;
    private readonly Command _hide;
    private bool _refreshing;
    private bool _pending;

    internal LayoutFloatingWindowCommands(LayoutAnchorableFloatingWindow model, Func<ILayoutElement?> getModel,
        Func<object?, bool> canClose, Func<object?, bool> canHide,
        Action<object?> close, Action<object?> hide)
    {
        _model = model;
        _getModel = getModel;
        _close = new(canClose, close, EnsureCurrentModel);
        _hide = new(canHide, hide, EnsureCurrentModel);
        model.PropertyChanged += ModelChanged;
        // A subclass's Model getter may depend on its unfinished constructor.
        Refresh(useCurrentModel: false);
    }

    internal ICommand Close => _close;
    internal ICommand Hide => _hide;

    private void ModelChanged(object? sender, PropertyChangedEventArgs e) => Refresh();
    private void RootUpdated(object? sender, EventArgs e) => Refresh();
    private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == LayoutItem.CloseCommandProperty || e.Property == LayoutAnchorableItem.HideCommandProperty ||
            e.Property == LayoutItem.CanCloseProperty || e.Property == LayoutAnchorableItem.CanHideProperty) Refresh();
    }
    private void CommandChanged(object? sender, EventArgs e)
    {
        _close.Refresh();
        _hide.Refresh();
    }

    private void EnsureCurrentModel()
    {
        if (_getModel is { } getModel && !ReferenceEquals(_model, getModel())) Refresh();
    }

    private void ObserveModel(ILayoutElement? model)
    {
        if (ReferenceEquals(_model, model)) return;
        if (_model is INotifyPropertyChanged previous) previous.PropertyChanged -= ModelChanged;
        _model = model;
        if (model is INotifyPropertyChanged current) current.PropertyChanged += ModelChanged;
    }

    private void Refresh(bool useCurrentModel = true)
    {
        if (_getModel is null) return;
        if (_refreshing) { _pending = true; return; }
        _refreshing = true;
        try
        {
            do
            {
                _pending = false;
                if (useCurrentModel) ObserveModel(_getModel());
                UpdateSubscriptions();
            } while (_pending && _getModel is not null);
        }
        finally { _refreshing = false; }
        CommandChanged(this, EventArgs.Empty);
    }

    private void UpdateSubscriptions()
    {
        LayoutRoot? root = _model?.Root as LayoutRoot;
        if (!ReferenceEquals(root, _root))
        {
            if (_root is not null) _root.Updated -= RootUpdated;
            _root = root;
            if (root is not null) root.Updated += RootUpdated;
        }
        UnsubscribeItems();
        if (_model is null || root?.Manager is not { } manager) return;
        foreach (LayoutContent content in LayoutFloatingWindowOperations.Snapshot<LayoutAnchorable>(_model))
        {
            if (manager.GetLayoutItemFromModel(content) is not { } item) continue;
            _items.Add(item);
            item.PropertyChanged += ItemChanged;
            Observe(item.CloseCommand);
            if (item is LayoutAnchorableItem tool) Observe(tool.HideCommand);
        }
    }

    private void Observe(ICommand? command)
    {
        if (command is not null && _observed.Add(command)) command.CanExecuteChanged += CommandChanged;
    }

    private void UnsubscribeItems()
    {
        foreach (LayoutItem item in _items) item.PropertyChanged -= ItemChanged;
        _items.Clear();
        foreach (ICommand command in _observed) command.CanExecuteChanged -= CommandChanged;
        _observed.Clear();
    }

    public void Dispose()
    {
        if (_getModel is null) return;
        _getModel = null;
        if (_model is INotifyPropertyChanged observable) observable.PropertyChanged -= ModelChanged;
        _model = null;
        if (_root is not null) _root.Updated -= RootUpdated;
        _root = null;
        UnsubscribeItems();
        try { _close.Dispose(); }
        finally { _hide.Dispose(); }
    }

    // ReactiveCommand owns execution. This ICommand boundary additionally
    // evaluates the caller's parameter against each current item command.
    private sealed class Command : ICommand, IDisposable
    {
        private Func<object?, bool>? _canExecute;
        private Action<object?>? _execute;
        private Action? _prepare;
        private readonly ReactiveCommand<object?, Unit> _command;

        internal Command(Func<object?, bool> canExecute, Action<object?> execute, Action prepare)
        {
            _canExecute = canExecute;
            _execute = execute;
            _prepare = prepare;
            _command = ReactiveCommand.Create<object?>(parameter => _execute?.Invoke(parameter),
                outputScheduler: ImmediateScheduler.Instance);
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter)
        {
            _prepare?.Invoke();
            return _canExecute?.Invoke(parameter) == true;
        }
        public void Execute(object? parameter)
        {
            _prepare?.Invoke();
            // Like upstream, direct execution delegates to item commands;
            // command sources query CanExecute before invoking this method.
            if (_execute is not null) ((ICommand)_command).Execute(parameter);
        }
        internal void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Dispose()
        {
            _canExecute = null;
            _execute = null;
            _prepare = null;
            try { Refresh(); }
            finally
            {
                CanExecuteChanged = null;
                _command.Dispose();
            }
        }
    }
}
