using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="DockPanel"/>.
    /// </summary>
    [PseudoClasses(":horizontal", ":vertical")]
    public class DockPanelSplitter : Thumb
    {
        private Control? _element;

        /// <summary>
        /// Defines the <see cref="Thickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<DockPanelSplitter, double>(nameof(Thickness), 4.0);

        /// <summary>
        /// Gets or sets the thickness (height or width, depending on orientation).
        /// </summary>
        /// <value>The thickness.</value>
        public double Thickness
        {
            get => GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether this splitter is horizontal.
        /// </summary>
        public bool IsHorizontal
        {
            get
            {
                var dock = GetDock(this);
                return dock == global::Avalonia.Controls.Dock.Top || dock == global::Avalonia.Controls.Dock.Bottom;
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

            UpdateHeightOrWidth();
            UpdateTargetElement();
        }

        private void AdjustHeight(double dy, global::Avalonia.Controls.Dock dock)
        {
            if (dock == global::Avalonia.Controls.Dock.Bottom)
            {
                dy = -dy;
            }
            SetTargetHeight(dy);
        }

        private void AdjustWidth(double dx, global::Avalonia.Controls.Dock dock)
        {
            if (dock == global::Avalonia.Controls.Dock.Right)
            {
                dx = -dx;
            }
            SetTargetWidth(dx);
        }

        private void SetTargetHeight(double dy)
        {
            if (_element is null)
            {
                return;
            }

            var height = _element.DesiredSize.Height + dy;

            if (height < _element.MinHeight)
            {
                height = _element.MinHeight;
            }

            if (height > _element.MaxHeight)
            {
                height = _element.MaxHeight;
            }

            var panel = GetPanel();
            if (panel is { })
            {
                var dock = GetDock(this);
                if (dock == global::Avalonia.Controls.Dock.Top && height > panel.DesiredSize.Height - Thickness)
                {
                    height = panel.DesiredSize.Height - Thickness;
                }

            }
            _element.Height = height;
        }

        private void SetTargetWidth(double dx)
        {
            if (_element is null)
            {
                return;
            }

            var width = _element.DesiredSize.Width + dx;

            if (width < _element.MinWidth)
            {
                width = _element.MinWidth;
            }

            if (width > _element.MaxWidth)
            {
                width = _element.MaxWidth;
            }

            var panel = GetPanel();
            if (panel is { })
            {
                var dock = GetDock(this);
                if (dock == global::Avalonia.Controls.Dock.Left && width > panel.DesiredSize.Width - Thickness)
                {
                    width = panel.DesiredSize.Width - Thickness;
                } 
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

        private global::Avalonia.Controls.Dock GetDock(Control control)
        {
            if (Parent is ContentPresenter presenter)
            {
                return DockPanel.GetDock(presenter);
            }
            return DockPanel.GetDock(control);
        }

        private Panel? GetPanel()
        {
            if (Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is Panel panel)
                {
                    return panel;
                }
            }
            else
            {
                if (Parent is Panel panel)
                {
                    return panel;
                }
            }

            return null;
        }

        private void UpdateTargetElement()
        {
            if (Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is not Panel panel)
                {
                    return;
                }

                var index = panel.Children.IndexOf(Parent);
                if (index > 0 && panel.Children.Count > 0)
                {
                    if (panel.Children[index - 1] is ContentPresenter contentPresenter)
                    {
                        _element = contentPresenter.Child as Control;
                    }
                }
            }
            else
            {
                if (Parent is not Panel panel)
                {
                    return;
                }

                var index = panel.Children.IndexOf(this);
                if (index > 0 && panel.Children.Count > 0)
                {
                    _element = panel.Children[index - 1] as Control;
                }
            }
        }
    }
}
