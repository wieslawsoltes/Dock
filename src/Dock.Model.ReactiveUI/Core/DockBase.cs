using System.Collections.Generic;
using System.Reactive;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core;

/// <summary>
/// Dock base class.
/// </summary>
[DataContract(IsReference = true)]
public abstract partial class DockBase : DockableBase, IDock
{
    internal readonly INavigateAdapter _navigateAdapter;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        _dock = DockMode.Center;
        GoBack = ReactiveCommand.Create(() => _navigateAdapter.GoBack());
        GoForward = ReactiveCommand.Create(() => _navigateAdapter.GoForward());
        Navigate = ReactiveCommand.Create<object>(root => _navigateAdapter.Navigate(root, true));
        Close = ReactiveCommand.Create(() => _navigateAdapter.Close());

        this.WhenAnyActiveDockable()
            .Subscribe(new AnonymousObserver<IDockable?>(x =>
            {
                Factory?.InitActiveDockable(x, this);
                this.RaisePropertyChanged(nameof(CanGoBack));
                this.RaisePropertyChanged(nameof(CanGoForward));
            }));

        this.WhenAnyFocusedDockable()
            .Subscribe(new AnonymousObserver<IDockable?>(x =>
            {
                Factory?.OnFocusedDockableChanged(x);
            }));
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? VisibleDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockable? ActiveDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockable? DefaultDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockable? FocusedDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial DockMode Dock { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsActive { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int OpenedDockablesCount { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool CanGoBack => _navigateAdapter.CanGoBack;

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool CanGoForward => _navigateAdapter.CanGoForward;

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand GoBack { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand GoForward { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand Navigate { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand Close { get; }
}
