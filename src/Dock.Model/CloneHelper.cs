using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model
{
    /// <summary>
    /// Clone helper.
    /// </summary>
    public static class CloneHelper
    {
        /// <summary>
        /// Clones dock properties.
        /// </summary>
        /// <param name="source">The source dock.</param>
        /// <param name="target">The target dock.</param>
        public static void CloneDockProperties(IDock source, IDock target)
        {
            target.Id = source.Id;
            target.Title = source.Title;
            target.Proportion = source.Proportion;
            target.IsActive = source.IsActive;
            target.IsCollapsable = source.IsCollapsable;

            if (source.VisibleDockables is not null)
            {
                target.VisibleDockables = source.Factory?.CreateList<IDockable>();
                if (target.VisibleDockables is not null)
                {
                    foreach (var visible in source.VisibleDockables)
                    {
                        var clone = visible.Clone();
                        if (clone is not null)
                        {
                            target.VisibleDockables.Add(clone);
                        }
                    }
                }
            }

            if (source.HiddenDockables is not null)
            {
                target.HiddenDockables = source.Factory?.CreateList<IDockable>();
                if (target.HiddenDockables is not null)
                {
                    foreach (var hidden in source.HiddenDockables)
                    {
                        var clone = hidden.Clone();
                        if (clone is not null)
                        {
                            target.HiddenDockables.Add(clone);
                        }
                    }
                }
            }

            if (source.PinnedDockables is not null)
            {
                target.PinnedDockables = source.Factory?.CreateList<IDockable>();
                if (target.PinnedDockables is not null)
                {
                    foreach (var pinned in source.PinnedDockables)
                    {
                        var clone = pinned.Clone();
                        if (clone is not null)
                        {
                            target.PinnedDockables.Add(clone);
                        }
                    }
                }
            }

            if (source.ActiveDockable is not null && source.VisibleDockables is not null)
            {
                var indexActiveDockable = source.VisibleDockables.IndexOf(source.ActiveDockable);
                if (indexActiveDockable >= 0)
                {
                    target.ActiveDockable = target.VisibleDockables?[indexActiveDockable];
                }
            }

            if (source.DefaultDockable is not null && source.VisibleDockables is not null)
            {
                var indexDefaultDockable = source.VisibleDockables.IndexOf(source.DefaultDockable);
                if (indexDefaultDockable >= 0)
                {
                    target.DefaultDockable = target.VisibleDockables?[indexDefaultDockable];
                }
            }

            if (source.FocusedDockable is not null && source.VisibleDockables is not null)
            {
                var indexFocusedDockable = source.VisibleDockables.IndexOf(source.FocusedDockable);
                if (indexFocusedDockable >= 0)
                {
                    target.FocusedDockable = target.VisibleDockables?[indexFocusedDockable];
                }
            }
        }

        /// <summary>
        /// Clones root dock properties.
        /// </summary>
        /// <param name="source">The source root dock.</param>
        /// <param name="target">The target root dock.</param>
        public static void CloneRootDockProperties(IRootDock source, IRootDock target)
        {
            target.IsFocusableRoot = source.IsFocusableRoot;

            target.Window = null;

            if (source.Windows is not null)
            {
                target.Windows = source.Factory?.CreateList<IDockWindow>();
                if (target.Windows is not null)
                {
                    foreach (var window in source.Windows)
                    {
                        var clone = window.Clone();
                        if (clone is not null)
                        {
                            target.Windows.Add(clone);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clones proportional dock properties.
        /// </summary>
        /// <param name="source">The source proportional dock.</param>
        /// <param name="target">The target proportional dock.</param>
        public static void CloneProportionalDockProperties(IProportionalDock source, IProportionalDock target)
        {
            target.Orientation = source.Orientation;
        }

        /// <summary>
        /// Clones dock window properties.
        /// </summary>
        /// <param name="source">The source dock window.</param>
        /// <param name="target">The target dock window.</param>
        public static void CloneDockWindowProperties(IDockWindow source, IDockWindow target)
        {
            target.Id = source.Id;
            target.X = source.X;
            target.Y = source.Y;
            target.Width = source.Width;
            target.Height = source.Height;
            target.Topmost = source.Topmost;
            target.Title = source.Title;

            if (source.Layout is not null)
            {
                target.Layout = (IRootDock?)source.Layout.Clone();
            }

            if (target.Layout is not null)
            {
                target.Layout.Window = target;
            }
        }

        /// <summary>
        /// Clones <see cref="IRootDock"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="IRootDock"/> class.</returns>
        public static IRootDock? CloneRootDock(IRootDock source)
        {
            var target = source.Factory?.CreateRootDock();

            if (target is not null)
            {
                CloneDockProperties(source, target);
                CloneRootDockProperties(source, target);
            }

            return target;
        }

        /// <summary>
        /// Clones <see cref="IProportionalDock"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="IProportionalDock"/> class.</returns>
        public static IProportionalDock? CloneProportionalDock(IProportionalDock source)
        {
            var target = source.Factory?.CreateProportionalDock();

            if (target is not null)
            {
                CloneDockProperties(source, target);
                CloneProportionalDockProperties(source, target);
            }

            return target;
        }

        /// <summary>
        /// Clones <see cref="ISplitterDock"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="ISplitterDock"/> class.</returns>
        public static ISplitterDock? CloneSplitterDock(ISplitterDock source)
        {
            var target = source.Factory?.CreateSplitterDock();

            if (target is not null)
            {
                CloneDockProperties(source, target);
            }

            return target;
        }

        /// <summary>
        /// Clones <see cref="IToolDock"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="IToolDock"/> class.</returns>
        public static IToolDock? CloneToolDock(IToolDock source)
        {
            var target = source.Factory?.CreateToolDock();

            if (target is not null)
            {
                target.Alignment = source.Alignment;
                target.IsExpanded = source.IsExpanded;
                target.AutoHide = source.AutoHide;

                CloneDockProperties(source, target);
            }

            return target;
        }

        /// <summary>
        /// Clones <see cref="IDocumentDock"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="IDocumentDock"/> class.</returns>
        public static IDocumentDock? CloneDocumentDock(IDocumentDock source)
        {
            var target = source.Factory?.CreateDocumentDock();

            if (target is not null)
            {
                CloneDockProperties(source, target);
            }

            return target;
        }

        /// <summary>
        /// Clones <see cref="IDockWindow"/> object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>TThe new instance or reference of the <see cref="IDockWindow"/> class.</returns>
        public static IDockWindow? CloneDockWindow(IDockWindow source)
        {
            source.Save();

            var target = source.Factory?.CreateDockWindow();
            if (target is not null)
            {
                CloneDockWindowProperties(source, target);
            }

            return target;
        }
    }
}
