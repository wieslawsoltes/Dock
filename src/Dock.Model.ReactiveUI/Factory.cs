using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI
{
    /// <summary>
    /// Factory.
    /// </summary>
    public class Factory : FactoryBase
    {
        /// <inheritdoc/>
        public override IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

        /// <inheritdoc/>
        public override IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

        /// <inheritdoc/>
        public override IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

        /// <inheritdoc/>
        public override IList<IDockControl> DockControls { get; }

        /// <summary>
        /// Initializes the new instance of <see cref="Factory"/> class.
        /// </summary>
        protected Factory()
        {
            VisibleDockableControls = new Dictionary<IDockable, IDockableControl>();
            PinnedDockableControls = new Dictionary<IDockable, IDockableControl>();
            TabDockableControls = new Dictionary<IDockable, IDockableControl>();
            DockControls = new ObservableCollection<IDockControl>();
        }

        /// <inheritdoc/>
        public override IList<T> CreateList<T>(params T[] items) => new ObservableCollection<T>(items);

        /// <inheritdoc/>
        public override IRootDock CreateRootDock() => new RootDock();

        /// <inheritdoc/>
        public override IProportionalDock CreateProportionalDock() => new ProportionalDock();

        /// <inheritdoc/>
        public override ISplitterDockable CreateSplitterDockable() => new SplitterDockable();

        /// <inheritdoc/>
        public override IToolDock CreateToolDock() => new ToolDock();

        /// <inheritdoc/>
        public override IDocumentDock CreateDocumentDock() => new DocumentDock();

        /// <inheritdoc/>
        public override IDockWindow CreateDockWindow() => new DockWindow();

        /// <inheritdoc/>
        public override IRootDock CreateLayout() => CreateRootDock();
    }
}
