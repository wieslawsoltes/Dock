using System;
using ReactiveUI;
using Splat;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ReactiveViewLocator : IViewLocator
{
    public IViewFor<TViewModel>? ResolveView<TViewModel>()
        where TViewModel : class
    {
        return ResolveView<TViewModel>(null);
    }

    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract)
        where TViewModel : class
    {
        var view = Locator.Current.GetService<IViewFor<TViewModel>>(contract);
        if (view is not null)
        {
            return view;
        }

        var fallback = GetFallback();
        return fallback?.ResolveView<TViewModel>(contract);
    }

    public IViewFor? ResolveView(object? instance)
    {
        return ResolveView(instance, null);
    }

    public IViewFor? ResolveView(object? instance, string? contract)
    {
        if (instance is null)
        {
            return null;
        }

        var viewType = typeof(IViewFor<>).MakeGenericType(instance.GetType());
        var view = Locator.Current.GetService(viewType, contract) as IViewFor;
        if (view is not null)
        {
            return view;
        }

        return GetFallback()?.ResolveView(instance, contract);
    }

    private IViewLocator? GetFallback()
    {
        var fallback = Locator.Current.GetService<IViewLocator>();
        if (fallback is null || ReferenceEquals(fallback, this))
        {
            return null;
        }

        return fallback;
    }
}
