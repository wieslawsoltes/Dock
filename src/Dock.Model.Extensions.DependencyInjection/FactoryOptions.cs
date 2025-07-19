using Dock.Model.Core;

namespace Dock.Model.Extensions.DependencyInjection;

/// <summary>
/// Options used when configuring Dock services.
/// </summary>
public class FactoryOptions
{
    /// <summary>
    /// Gets or sets the default layout instance.
    /// </summary>
    public IRootDock? Layout { get; set; }
}

