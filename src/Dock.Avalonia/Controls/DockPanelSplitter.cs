// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="DockPanel"/>.
    /// </summary>
    public class DockPanelSplitter : Thumb
    {
        private Control _element;
        private Size _previousParentSize;
        private bool _initialised;

        /// <summary>
        /// Defines the <see cref="Thickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<DockPanelSplitter, double>(nameof(Thickness), 4.0);

        /// <summary>
        /// Defines the <see cref="ProportionalResize"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ProportionalResizeProperty =
            AvaloniaProperty.Register<DockPanelSplitter, bool>(nameof(ProportionalResize), true);

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
        /// Initializes a new instance of the <see cref="DockPanelSplitter" /> class.
        /// </summary>
        public DockPanelSplitter()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this splitter is horizontal.
        /// </summary>
        public bool IsHorizontal
        {
            get
            {
                var dock = GetDock(this);
                return dock == Dock.Top || dock == Dock.Bottom;
            }
        }

        /// <inheritdoc/>
        protected override void OnDragDelta(VectorEventArgs e)
        {
            var dock = GetDock(this);
            if (IsHorizontal)
            {
                AdjustHeight(e.Vector.Y, dock);
            }
            else
            {
                AdjustWidth(e.Vector.X, dock);
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

        private void AdjustHeight(double dy, Dock dock)
        {
            if (dock == Dock.Bottom)
            {
                dy = -dy;
            }
            SetTargetHeight(dy);
        }

        private void AdjustWidth(double dx, Dock dock)
        {
            if (dock == Dock.Right)
            {
                dx = -dx;
            }
            SetTargetWidth(dx);
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
            var dock = GetDock(this);
            if (dock == Dock.Top && height > panel.DesiredSize.Height - Thickness)
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
            var dock = GetDock(this);
            if (dock == Dock.Left && width > panel.DesiredSize.Width - Thickness)
            {
                width = panel.DesiredSize.Width - Thickness;
            }

            _element.Width = width;
        }

        private void UpdateHeightOrWidth()
        {
            if (IsHorizontal)
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

        private Dock GetDock(Control control)
        {
            if (this.Parent is ContentPresenter presenter)
            {
                return DockPanel.GetDock(presenter);
            }
            return DockPanel.GetDock(control);
        }

        private Panel GetPanel()
        {
            if (this.Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is Panel panel)
                {
                    return panel;
                }
            }
            else
            {
                if (this.Parent is Panel panel)
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
