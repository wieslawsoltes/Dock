// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Dock.Model.Controls;
using ReactiveUI.Legacy;

namespace Dock.Model
{
    /// <summary>
    /// Factory.
    /// </summary>
    public abstract class Factory : FactoryBase
    {
        /// <inheritdoc/>
        public override IList<T> CreateList<T>(params T[] items)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new ReactiveList<T>(items);
#pragma warning restore CS0618 // Type or member is obsolete
        }

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
        public override ITool CreateTool() => new Tool();

        /// <inheritdoc/>
        public override IDocument CreateDocument() => new Document();
    }
}
