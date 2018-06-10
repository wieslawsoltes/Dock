// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model;

namespace AvaloniaDemo.ViewModels
{
    /// <summary>
    /// Layout editor contract.
    /// </summary>
    public interface ILayoutEditor
    {
        /// <summary>
        /// Adds <see cref="Controls.ILayoutDock"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddLayout(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IRootDock"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddRoot(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.ISplitterDock"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddSplitter(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IDocumentDock"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddDocument(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IToolDock"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddTool(IDock dock);

        /// <summary>
        /// Adds <see cref="IView"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddView(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IToolTab"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddToolTab(IDock dock);

        /// <summary>
        /// Adds <see cref="Controls.IDocumentTab"/> into dock <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void AddDocumentTab(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="IView"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolTab"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolTabBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentTab"/> before dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentTabBefore(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ILayoutDock"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertLayoutAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IRootDock"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertRootAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.ISplitterDock"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertSplitterAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentDock"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolDock"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="IView"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertViewAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IToolTab"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertToolTabAfter(IDock dock);

        /// <summary>
        /// Insert <see cref="Controls.IDocumentTab"/> after dock in parent <see cref="IDock.Views"/> collection.
        /// </summary>
        /// <param name="dock">The dock object.</param>
        void InsertDocumentTabAfter(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.ILayoutDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToLayout(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IRootDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToRoot(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.ISplitterDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToSplitter(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IDocumentDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToDocument(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IToolDock"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToTool(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="IView"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToView(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IToolTab"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToToolTab(IDock dock);

        /// <summary>
        /// Converts dock to <see cref="Controls.IDocumentTab"/> type.
        /// </summary>
        /// <param name="dock">The dock to convert.</param>
        void ConvertToDocumentTab(IDock dock);
    }
}
