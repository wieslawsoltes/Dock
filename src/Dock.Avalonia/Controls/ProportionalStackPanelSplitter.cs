// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
    /// </summary>
    public class ProportionalStackPanelSplitter : Thumb
    {
        private Size _previousParentSize;
        private bool _initialised;

        /// <summary>
        /// Defines the Proportion attached property.
        /// </summary>
        public static readonly AttachedProperty<double> ProportionProperty =
            AvaloniaProperty.RegisterAttached<ProportionalStackPanelSplitter, IControl, double>("Proportion", double.NaN, false, BindingMode.TwoWay);

        /// <summary>
        /// Gets the value of the Proportion attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The Proportion attached property.</returns>
        public static double GetProportion(IControl control)
        {
            return control.GetValue(ProportionProperty);
        }

        /// <summary>
        /// Sets the value of the Proportion attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the Proportion property.</param>
        public static void SetProportion(IControl control, double value)
        {
            control.SetValue(ProportionProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Thickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<ProportionalStackPanelSplitter, double>(nameof(Thickness), 4.0);

        /// <summary>
        /// Defines the <see cref="ProportionalResize"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ProportionalResizeProperty =
            AvaloniaProperty.Register<ProportionalStackPanelSplitter, bool>(nameof(ProportionalResize), true);

        /// <summary>
        /// Gets or sets a value indicating whether to resize elements proportionally.
        /// </summary>
        /// <remarks>Set to <c>false</c> if you don't want the element to be resized when the parent is resized.</remarks>
        public bool ProportionalResize
        {
            get => GetValue(ProportionalResizeProperty);
            set => SetValue(ProportionalResizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the thickness (height or width, depending on orientation).
        /// </summary>
        /// <value>The thickness.</value>
        public double Thickness
        {
            get { return GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProportionalStackPanelSplitter" /> class.
        /// </summary>
        public ProportionalStackPanelSplitter()
        {
        }

        /// <inheritdoc/>
        protected override void OnDragDelta(VectorEventArgs e)
        {
            if (GetPanel().Orientation == Orientation.Vertical)
            {
                SetTargetProportion(e.Vector.Y);
            }
            else
            {
                SetTargetProportion(e.Vector.X);
            }
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            var panel = GetPanel();

            _previousParentSize = panel.Bounds.Size;

            panel.LayoutUpdated += (sender, ee) =>
            {
                if (!this.ProportionalResize)
                {
                    return;
                }

                var target = GetTargetElement();

                if (_initialised && target != null && target.IsArrangeValid && target.IsMeasureValid)
                {
                    var dSize = new Size(panel.Bounds.Size.Width / _previousParentSize.Width, panel.Bounds.Size.Height / _previousParentSize.Height);

                    if (!double.IsNaN(dSize.Width) && !double.IsInfinity(dSize.Width))
                    {
                        SetTargetWidth((target.DesiredSize.Width * dSize.Width) - target.DesiredSize.Width, panel, target);
                    }

                    if (!double.IsInfinity(dSize.Height) && !double.IsNaN(dSize.Height))
                    {
                        SetTargetHeight((target.DesiredSize.Height * dSize.Height) - target.DesiredSize.Height, panel, target);
                    }
                }

                _previousParentSize = panel.Bounds.Size;
                _initialised = true;
            };

            UpdateHeightOrWidth();
        }

        private void SetTargetProportion(double dragDelta)
        {
            var target = GetTargetElement();
            var panel = GetPanel();
            var children = panel.GetChildren();

            int index = children.IndexOf(this) + 1;

            var child = children[index];

            var targetElementProportion = GetProportion(target);
            var neighbourProportion = GetProportion(child);

            var dProportion = dragDelta / (panel.Orientation == Orientation.Vertical ? panel.Bounds.Height : panel.Bounds.Width);

            if (targetElementProportion + dProportion < 0)
            {
                dProportion = -targetElementProportion;
            }

            if (neighbourProportion - dProportion < 0)
            {
                dProportion = +neighbourProportion;
            }

            targetElementProportion += dProportion;
            neighbourProportion -= dProportion;

            SetProportion(target, targetElementProportion);

            SetProportion(child, neighbourProportion);

            panel.InvalidateMeasure();
            panel.InvalidateArrange();
        }

        private void SetTargetHeight(double dy, ProportionalStackPanel panel, Control target)
        {
            double height = target.Height + dy;

            if (height < target.MinHeight)
            {
                height = target.MinHeight;
            }

            if (height > target.MaxHeight)
            {
                height = target.MaxHeight;
            }

            if (panel.Orientation == Orientation.Vertical && height > panel.DesiredSize.Height - Thickness)
            {
                height = panel.DesiredSize.Height - Thickness;
            }

            target.Height = height;
        }

        private void SetTargetWidth(double dx, ProportionalStackPanel panel, Control target)
        {
            double width = target.Width + dx;

            if (width < target.MinWidth)
            {
                width = target.MinWidth;
            }

            if (width > target.MaxWidth)
            {
                width = target.MaxWidth;
            }

            if (panel.Orientation == Orientation.Horizontal && width > panel.DesiredSize.Width - Thickness)
            {
                width = panel.DesiredSize.Width - Thickness;
            }

            target.Width = width;
        }

        private void UpdateHeightOrWidth()
        {
            if (GetPanel().Orientation == Orientation.Vertical)
            {
                Height = Thickness;
                Width = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                PseudoClasses.Add(":horizontal");
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
                PseudoClasses.Add(":vertical");
            }
        }

        private ProportionalStackPanel GetPanel()
        {
            if (Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is ProportionalStackPanel panel)
                {
                    return panel;
                }
            }
            else if (this.GetVisualParent() is ProportionalStackPanel psp)
            {
                return psp;
            }

            return null;
        }

        private Control GetTargetElement()
        {
            if (Parent is ContentPresenter presenter)
            {
                if (!(presenter.GetVisualParent() is Panel panel))
                {
                    return null;
                }

                int index = panel.Children.IndexOf(Parent);
                if (index > 0 && panel.Children.Count > 0)
                {
                    return (panel.Children[index - 1] as ContentPresenter).Child as Control;
                }
            }
            else
            {
                var panel = GetPanel();

                int index = panel.Children.IndexOf(this);
                if (index > 0 && panel.Children.Count > 0)
                {
                    return panel.Children[index - 1] as Control;
                }
            }

            return null;
        }
    }
}
