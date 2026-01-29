using System;
using ReactiveUI;
using Splat;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ReactiveViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel is null)
        {
            return null;
        }

        var viewType = typeof(IViewFor<>).MakeGenericType(viewModel.GetType());
        var view = Locator.Current.GetService(viewType, contract) as IViewFor;
        if (view is not null)
        {
            return view;
        }

        var fallback = Locator.Current.GetService<IViewLocator>();
        if (fallback is null || ReferenceEquals(fallback, this))
        {
            return null;
        }

        return fallback.ResolveView(viewModel, contract);
    }
}
