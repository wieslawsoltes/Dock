using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI.Reactive;
using ReactiveUI;

namespace DockReactiveUIDiSample;

public class ViewLocator : IDataTemplate, IViewLocator
{
    private readonly IServiceProvider _provider;

    public ViewLocator(IServiceProvider provider)
    {
        _provider = provider;
    }

    private IViewFor? Resolve(object viewModel)
    {
        var vmType = viewModel.GetType();
        var serviceType = typeof(IViewFor<>).MakeGenericType(vmType);
        if (_provider.GetService(serviceType) is IViewFor view)
        {
            view.ViewModel = viewModel;
            return view;
        }

        var viewName = vmType.FullName?.Replace("ViewModel", "View");
        if (viewName is null)
            return null;

        var viewType = Type.GetType(viewName);
        if (viewType != null && _provider.GetService(viewType) is IViewFor view2)
        {
            view2.ViewModel = viewModel;
            return view2;
        }

        return null;
    }

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        if (Resolve(data) is IViewFor view && view is Control control)
        {
            return control;
        }

        var viewName = data.GetType().FullName?.Replace("ViewModel", "View");
        return new TextBlock { Text = $"Not Found: {viewName}" };
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        return data is IDockable || Resolve(data) is not null;
    }

    public IViewFor<TViewModel>? ResolveView<TViewModel>()
        where TViewModel : class
    {
        return ResolveView<TViewModel>(null);
    }

    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract)
        where TViewModel : class
    {
        return _provider.GetService(typeof(IViewFor<TViewModel>)) as IViewFor<TViewModel>;
    }

    public IViewFor? ResolveView(object? instance)
    {
        return ResolveView(instance, null);
    }

    public IViewFor? ResolveView(object? instance, string? contract)
    {
        return instance is null ? null : Resolve(instance);
    }
}
