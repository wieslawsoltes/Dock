using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Avalonia.Internal;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Control used to track associated to <see cref="IDockable"/> control state.
    /// </summary>
    public class DockableControl : Panel, IDockableControl
    {
        internal static readonly Dictionary<IDockable, IDockableControl> s_visibleDockableControls = new();
        internal static readonly Dictionary<IDockable, IDockableControl> s_pinnedDockableControls = new();
        internal static readonly Dictionary<IDockable, IDockableControl> s_tabDockableControls = new();

        private IDisposable? _boundsDisposable;

        /// <summary>
        /// Defines the <see cref="TrackingMode"/> property.
        /// </summary>
        public static readonly StyledProperty<TrackingMode> TrackingModeProperty = 
            AvaloniaProperty.Register<DockableControl, TrackingMode>(nameof(TrackingMode));

        /// <summary>
        /// Gets or sets dockable tracking mode.
        /// </summary>
        public TrackingMode TrackingMode
        {
            get => GetValue(TrackingModeProperty);
            set => SetValue(TrackingModeProperty, value);
        }

        /// <inheritdoc/>
        public IDictionary<IDockable, IDockableControl> VisibleDockableControls => s_visibleDockableControls;
  
        /// <inheritdoc/>
        public IDictionary<IDockable, IDockableControl> PinnedDockableControls => s_pinnedDockableControls;
  
        /// <inheritdoc/>
        public IDictionary<IDockable, IDockableControl> TabDockableControls => s_tabDockableControls;

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            Register();

            AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            AddHandler(PointerMovedEvent, Moved, RoutingStrategies.Tunnel);

            _boundsDisposable = this.GetObservable(BoundsProperty).Subscribe((bounds) =>
            {
                if (DataContext is not IDockable dockable)
                {
                    return;
                }

                SetBoundsTracking(dockable, bounds);
            });
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            UnRegister();

            RemoveHandler(PointerPressedEvent, Pressed);

            _boundsDisposable?.Dispose();
        }

        private void Register()
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

            switch (TrackingMode)
            {
                case TrackingMode.Visible:
                    s_visibleDockableControls.Add(dockable, this);
                    break;
                case TrackingMode.Pinned:
                    s_pinnedDockableControls.Add(dockable, this);
                    break;
                case TrackingMode.Tab:
                    s_tabDockableControls.Add(dockable, this);
                    break;
            }
        }

        private void UnRegister()
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

            switch (TrackingMode)
            {
                case TrackingMode.Visible:
                    s_visibleDockableControls.Remove(dockable);
                    break;
                case TrackingMode.Pinned:
                    s_pinnedDockableControls.Remove(dockable);
                    break;
                case TrackingMode.Tab:
                    s_tabDockableControls.Remove(dockable);
                    break;
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

            SetPointerTracking(dockable, e);
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

            SetPointerTracking(dockable, e);
        }

        private void SetBoundsTracking(IDockable dockable, Rect bounds)
        {
            var x = bounds.X;
            var y = bounds.Y;
            var width = bounds.Width;
            var height = bounds.Height;

            var translatedPosition = this.TranslatePoint(bounds.Position, VisualRoot);
            if (translatedPosition.HasValue)
            {
                x = translatedPosition.Value.X;
                y = translatedPosition.Value.Y;
            }

            switch (TrackingMode)
            {
                case TrackingMode.Visible:
                    dockable.SetVisibleBounds(x, y, width, height);
                    break;
                case TrackingMode.Pinned:
                    dockable.SetPinnedBounds(x, y, width, height);
                    break;
                case TrackingMode.Tab:
                    dockable.SetTabBounds(x, y, width, height);
                    break;
            }
        }

        private void SetPointerTracking(IDockable dockable, PointerEventArgs e)
        {
            var position = e.GetPosition(this);
            var screenPosition = DockHelpers.ToDockPoint(this.PointToScreen(position).ToPoint(1.0));

            dockable.SetPointerPosition(position.X, position.Y);
            dockable.SetPointerScreenPosition(screenPosition.X, screenPosition.Y);
        }
    }
}
