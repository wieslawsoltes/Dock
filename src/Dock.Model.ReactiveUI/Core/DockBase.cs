// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
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
    private bool _enableGlobalDocking = true;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        _canCloseLastDockable = true;
        _openedDockablesCount = 0;

        GoBack = ReactiveCommand.Create(() => _navigateAdapter.GoBack());
        GoForward = ReactiveCommand.Create(() => _navigateAdapter.GoForward());
        Navigate = ReactiveCommand.Create<object>(root => _navigateAdapter.Navigate(root, true));
        Close = ReactiveCommand.Create(() => _navigateAdapter.Close());

        this.WhenAnyValue(x => x.ActiveDockable)
            .Subscribe(new AnonymousObserver<IDockable?>(x =>
            {
                Factory?.InitActiveDockable(x, this);
                this.RaisePropertyChanged(nameof(CanGoBack));
                this.RaisePropertyChanged(nameof(CanGoForward));
            }));

        this.WhenAnyValue(x => x.FocusedDockable)
            .Subscribe(new AnonymousObserver<IDockable?>(x =>
            {
                Factory?.OnFocusedDockableChanged(x);
            }));
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IList<IDockable>? VisibleDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? ActiveDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? DefaultDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? FocusedDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsActive { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int OpenedDockablesCount { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanCloseLastDockable { get; set; }

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableGlobalDocking
    {
        get => _enableGlobalDocking;
        set => this.RaiseAndSetIfChanged(ref _enableGlobalDocking, value);
    }
}
