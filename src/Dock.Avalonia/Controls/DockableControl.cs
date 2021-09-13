using System;
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
        private IDisposable? _boundsDisposable;
        private IDisposable? _dataContextDisposable;
        private IDockable? _currentDockable;

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
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _dataContextDisposable = this.GetObservable(DataContextProperty).Subscribe((dataContext) =>
            {
                if (_currentDockable is not null)
                {
                    UnRegister(_currentDockable);
                    _currentDockable = null;
                }

                if (dataContext is IDockable dockableChanged)
                {
                    _currentDockable = dockableChanged;
                    Register(dockableChanged);
                }
            });

            AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
            AddHandler(PointerMovedEvent, MovedHandler, RoutingStrategies.Tunnel);

            _boundsDisposable = this.GetObservable(BoundsProperty).Subscribe(SetBoundsTracking);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (_currentDockable is not null)
            {
                UnRegister(_currentDockable);
                _currentDockable = null;
            }

            RemoveHandler(PointerPressedEvent, PressedHandler);
            RemoveHandler(PointerMovedEvent, MovedHandler);

            _boundsDisposable?.Dispose();
            _dataContextDisposable?.Dispose();
        }

        private void Register(IDockable dockable)
        {
            switch (TrackingMode)
            {
                case TrackingMode.Visible:
                    if (dockable.Factory is not null)
                    {
                        dockable.Factory.VisibleDockableControls[dockable] = this;
                    }
                    break;
                case TrackingMode.Pinned:
                    if (dockable.Factory is not null)
                    {
                        dockable.Factory.PinnedDockableControls[dockable] = this;
                    }
                    break;
                case TrackingMode.Tab:
                    if (dockable.Factory is not null)
                    {
                        dockable.Factory.TabDockableControls[dockable] = this;
                    }
                    break;
            }
        }

        private void UnRegister(IDockable dockable)
        {
            switch (TrackingMode)
            {
                case TrackingMode.Visible:
                    dockable.Factory?.VisibleDockableControls.Remove(dockable);
                    break;
                case TrackingMode.Pinned:
                    dockable.Factory?.PinnedDockableControls.Remove(dockable);
                    break;
                case TrackingMode.Tab:
                    dockable.Factory?.TabDockableControls.Remove(dockable);
                    break;
            }
        }

        private void PressedHandler(object? sender, PointerPressedEventArgs e)
        {
            SetPointerTracking(e);
        }

        private void MovedHandler(object? sender, PointerEventArgs e)
        {
            SetPointerTracking(e);
        }

        private void SetBoundsTracking(Rect bounds)
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

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

        private void SetPointerTracking(PointerEventArgs e)
        {
            if (DataContext is not IDockable dockable)
            {
                return;
            }

            var position = e.GetPosition(this);

            if (this.VisualRoot is null)
            {
                return;
            }
            var screenPoint = DockHelpers.ToDockPoint(this.PointToScreen(position).ToPoint(1.0));

            dockable.SetPointerPosition(position.X, position.Y);
            dockable.SetPointerScreenPosition(screenPoint.X, screenPoint.Y);
        }
    }
}
