// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockControl, IDock>(nameof(Layout));

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        /// <value>The layout.</value>
        [Content]
        public IDock Layout
        {
            get { return GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        [Conditional("DEBUG")]
        private void Print(IVisual relativeTo, PointerEventArgs e, string name)
        {
            var point = e.GetPosition(relativeTo);

            if (VisualRoot is IRenderRoot root)
            {
                var visuals = root.GetVisualsAt(point, x => (!(x is AdornerLayer) && x.IsVisible)).ToList();
                if (visuals.Count > 0)
                {
                    Debug.WriteLine($"{name} : {root.GetType().Name} : {point}");
                    foreach (var visual in visuals)
                    {
                        if (visual is IControl control)
                        {
                            Debug.WriteLine($"- {control.GetType().Name} : {control.DataContext?.GetType().Name}");
                        }
                        else
                        {
                            Debug.WriteLine($"- {visual.GetType().Name}");
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Print(this, e, nameof(OnPointerPressed));
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            Print(this, e, nameof(OnPointerReleased));
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            Print(this, e, nameof(OnPointerMoved));
        }

        /// <inheritdoc/>
        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            Print(this, e, nameof(OnPointerEnter));
        }

        /// <inheritdoc/>
        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);
            Print(this, e, nameof(OnPointerLeave));
        }

        /// <inheritdoc/>
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            Print(this, e, nameof(OnPointerWheelChanged));
        }
    }
}
