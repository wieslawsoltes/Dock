using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Controls;

/// <summary>
/// Root dock that supports <see cref="IRoutableViewModel"/> for ReactiveUI navigation.
/// </summary>
[DataContract(IsReference = true)]
public class RoutableRootDock : RootDock, IRoutableViewModel, IScreen
{
    /// <summary>
    /// Initializes new instance of the <see cref="RoutableRootDock"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="url">Optional url segment.</param>
    public RoutableRootDock(IScreen host, string? url = null)
    {
        HostScreen = host;
        UrlPathSegment = url ?? GetType().Name;
    }

    /// <summary>
    /// Gets router used for nested navigation.
    /// </summary>
    [IgnoreDataMember]
    public RoutingState Router { get; } = new RoutingState();

    /// <inheritdoc/>
    [IgnoreDataMember]
    public string UrlPathSegment { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IScreen HostScreen { get; }
}
