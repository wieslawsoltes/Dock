using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Factory.
    /// </summary>
    public class Factory : FactoryBase
    {
        /// <inheritdoc/>
        public override IList<T> CreateList<T>(params T[] items) => new ObservableCollection<T>(items);

        /// <inheritdoc/>
        public override IRootDock CreateRootDock() => new RootDock();

        /// <inheritdoc/>
        public override IPinDock CreatePinDock() => new PinDock();

        /// <inheritdoc/>
        public override IProportionalDock CreateProportionalDock() => new ProportionalDock();

        /// <inheritdoc/>
        public override ISplitterDock CreateSplitterDock() => new SplitterDock();

        /// <inheritdoc/>
        public override IToolDock CreateToolDock() => new ToolDock();

        /// <inheritdoc/>
        public override IDocumentDock CreateDocumentDock() => new DocumentDock();

        /// <inheritdoc/>
        public override IDockWindow CreateDockWindow() => new DockWindow();

        /// <inheritdoc/>
        public override IDock? CreateLayout() => new RootDock();
    }
}
