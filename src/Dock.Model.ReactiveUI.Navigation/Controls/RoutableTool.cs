using System.Runtime.Serialization;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Controls;

/// <summary>
/// Tool that supports ReactiveUI routing and acts as a screen for nested navigation.
/// </summary>
[DataContract(IsReference = true)]
public class RoutableTool : Tool, IRoutableViewModel, IScreen
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoutableTool"/> class.
    /// </summary>
    /// <param name="host">The host screen.</param>
    /// <param name="url">Optional url segment.</param>
    public RoutableTool(IScreen host, string? url = null)
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
