using System.Runtime.Serialization;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Core;

/// <summary>
/// Dock base class that also implements <see cref="IRoutableViewModel"/>.
/// </summary>
[DataContract(IsReference = true)]
public abstract class RoutableDockBase : DockBase, IRoutableViewModel
{
    /// <summary>
    /// Initializes new instance of the <see cref="RoutableDockBase"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="url">Optional url segment.</param>
    protected RoutableDockBase(IScreen host, string? url = null)
    {
        HostScreen = host;
        UrlPathSegment = url ?? GetType().Name;
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public string UrlPathSegment { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IScreen HostScreen { get; }
}
