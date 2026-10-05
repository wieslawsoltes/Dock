using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI.Reactive;

namespace DockReactiveUIDiSample;

/// <summary>Resolves registered views without runtime type construction.</summary>
public class ViewLocator : IDataTemplate, IViewLocator
{
    private readonly IReadOnlyDictionary<Type, Func<IViewFor>> _viewFactories;

    /// <summary>Creates a locator using view factories from the composition root.</summary>
    /// <param name="viewFactories">Factories keyed by their view model type.</param>
    public ViewLocator(IReadOnlyDictionary<Type, Func<IViewFor>> viewFactories)
    {
        _viewFactories = viewFactories;
    }

    /// <inheritdoc />
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        return ResolveView(data, null) as Control
            ?? new TextBlock { Text = $"Not Found: {data.GetType().FullName}" };
    }

    /// <inheritdoc />
    public bool Match(object? data)
        => data is IDockable || (data is not null && _viewFactories.ContainsKey(data.GetType()));

    /// <inheritdoc />
    public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class
        => ResolveView((object?)viewModel, contract);

    /// <inheritdoc />
    public IViewFor? ResolveView(object? viewModel, string? contract)
    {
        if (viewModel is null || !_viewFactories.TryGetValue(viewModel.GetType(), out var factory))
        {
            return null;
        }

        var view = factory();
        view.ViewModel = viewModel;
        return view;
    }

    /// <inheritdoc />
    public IViewFor? ResolveViewUnsafe(object? viewModel, string? contract)
        => ResolveView(viewModel, contract);
}
