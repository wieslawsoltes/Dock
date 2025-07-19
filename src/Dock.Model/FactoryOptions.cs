using System;
using System.Collections.Generic;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Configuration options for <see cref="FactoryBase"/>.
/// </summary>
public class FactoryOptions
{
    /// <summary>
    /// Optional context locator dictionary.
    /// </summary>
    public Dictionary<string, Func<object?>>? ContextLocator { get; set; }

    /// <summary>
    /// Optional dockable locator dictionary.
    /// </summary>
    public IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

    /// <summary>
    /// Optional host window locator dictionary.
    /// </summary>
    public Dictionary<string, Func<IHostWindow?>>? HostWindowLocator { get; set; }

    /// <summary>
    /// Optional hide tools flag.
    /// </summary>
    public bool? HideToolsOnClose { get; set; }

    /// <summary>
    /// Optional hide documents flag.
    /// </summary>
    public bool? HideDocumentsOnClose { get; set; }
}

