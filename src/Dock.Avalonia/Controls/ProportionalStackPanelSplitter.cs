// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="DockPanel"/>.
    /// </summary>
    public class ProportionalStackPanelSplitter : Thumb
    {
        private Control _element;
        private Size _previousParentSize;
        private bool _initialised;

        /// <summary>
        /// Defines the Proportion attached property.
        /// </summary>
        public static readonly AttachedProperty<double> ProportionProperty =
            AvaloniaProperty.RegisterAttached<DockPanel, IControl, double>("Proportion", double.NaN);

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
        /// <param name="value">The value of the Dock property.</param>
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
            if(GetPanel().Orientation == Orientation.Vertical)
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

                var proportion = GetProportion(_element);

                if (_initialised && _element.IsArrangeValid && _element.IsMeasureValid)
                {
                    var dSize = new Size(panel.Bounds.Size.Width / _previousParentSize.Width, panel.Bounds.Size.Height / _previousParentSize.Height);

                    if (!double.IsNaN(dSize.Width) && !double.IsInfinity(dSize.Width))
                    {
                        this.SetTargetWidth((_element.DesiredSize.Width * dSize.Width) - _element.DesiredSize.Width);
                    }

                    if (!double.IsInfinity(dSize.Height) && !double.IsNaN(dSize.Height))
                    {
                        this.SetTargetHeight((_element.DesiredSize.Height * dSize.Height) - _element.DesiredSize.Height);
                    }
                }

                _previousParentSize = panel.Bounds.Size;
                _initialised = true;
            };

            UpdateHeightOrWidth();
            UpdateTargetElement();
        }

        private void SetTargetProportion (double dy)
        {
            var proportion = GetProportion(_element);

            var panel = GetPanel();

            var dProportion = dy / panel.Bounds.Height;

            proportion += dProportion;

            SetProportion(_element, proportion);

            int index = panel.Children.IndexOf(this) + 1;

            var child = panel.Children[index];

            var currentProportion = GetProportion(child);

            currentProportion -= dProportion;

            SetProportion(child, currentProportion);

            panel.InvalidateMeasure();
            panel.InvalidateArrange();
        }

        private void SetTargetHeight(double dy)
        {
            double height = _element.Height + dy;

            if (height < _element.MinHeight)
            {
                height = _element.MinHeight;
            }

            if (height > _element.MaxHeight)
            {
                height = _element.MaxHeight;
            }

            var panel = GetPanel();
            if (panel.Orientation == Orientation.Vertical && height > panel.DesiredSize.Height - Thickness)
            {
                height = panel.DesiredSize.Height - Thickness;
            }

            _element.Height = height;
        }

        private void SetTargetWidth(double dx)
        {
            double width = _element.Width + dx;

            if (width < _element.MinWidth)
            {
                width = _element.MinWidth;
            }

            if (width > _element.MaxWidth)
            {
                width = _element.MaxWidth;
            }

            var panel = GetPanel();
            if (panel.Orientation == Orientation.Horizontal && width > panel.DesiredSize.Width - Thickness)
            {
                width = panel.DesiredSize.Width - Thickness;
            }

            _element.Width = width;
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
            if (this.Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is ProportionalStackPanel panel)
                {
                    return panel;
                }
            }
            else
            {
                if (this.Parent is ProportionalStackPanel panel)
                {
                    return panel;
                }
            }

            return null;
        }

        private void UpdateTargetElement()
        {
            if (this.Parent is ContentPresenter presenter)
            {
                if (!(presenter.GetVisualParent() is Panel panel))
                {
                    return;
                }

                int index = panel.Children.IndexOf(this.Parent);
                if (index > 0 && panel.Children.Count > 0)
                {
                    _element = (panel.Children[index - 1] as ContentPresenter).Child as Control;
                }
            }
            else
            {
                if (!(this.Parent is Panel panel))
                {
                    return;
                }

                int index = panel.Children.IndexOf(this);
                if (index > 0 && panel.Children.Count > 0)
                {
                    _element = panel.Children[index - 1] as Control;
                }
            }
        }
    }
}
