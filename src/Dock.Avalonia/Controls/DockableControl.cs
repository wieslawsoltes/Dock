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
 
            _boundsDisposable = this.GetObservable(BoundsProperty).Subscribe((x) =>
            {
                if (DataContext is IDockable dockable)
                {
                    switch (TrackingMode)
                    {
                        case TrackingMode.Visible:
                            dockable.SetVisibleBounds(x.X, x.Y, x.Width, x.Height);
                            break;
                        case TrackingMode.Pinned:
                            dockable.SetPinnedBounds(x.X, x.Y, x.Width, x.Height);
                            break;
                        case TrackingMode.Tab:
                            dockable.SetTabBounds(x.X, x.Y, x.Width, x.Height);
                            break;
                        
                    }
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
