using System;
using Avalonia;
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Defines tracking mode.
    /// </summary>
    public enum TrackingMode
    {
        /// <summary>
        /// Visible mode.
        /// </summary>
        Visible,
        /// <summary>
        /// Pinned mode.
        /// </summary>
        Pinned,
        /// <summary>
        /// Tab mode.
        /// </summary>
        Tab
    }

    /// <summary>
    /// Control used to track associated to <see cref="IDockable"/> control state.
    /// </summary>
    public class DockableControl : Panel
    {
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
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _boundsDisposable = this.GetObservable(BoundsProperty).Subscribe((bounds) =>
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
            });
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _boundsDisposable?.Dispose();
        }
    }
}
