using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.MarkupExtension.UnitTests;

internal class TestServiceProvider : IServiceProvider
{
    private readonly IUriContext? _uriContext;
    private readonly INameScope? _nameScope;

    public TestServiceProvider(IUriContext? uriContext = null, INameScope? nameScope = null)
    {
        _uriContext = uriContext;
        _nameScope = nameScope;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IUriContext))
            return _uriContext;
        if (serviceType == typeof(INameScope))
            return _nameScope;
        return null;
    }
}
