using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.ProportionalStackPanel
{
    /// <summary>
    /// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
    /// </summary>
    [PseudoClasses(":horizontal", ":vertical")]
    public class ProportionalStackPanelSplitter : Thumb
    {
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
        /// Defines the MinimumProportionSize attached property.
        /// </summary>
        public static readonly AttachedProperty<double> MinimumProportionSizeProperty =
            AvaloniaProperty.RegisterAttached<ProportionalStackPanelSplitter, IControl, double>("MinimumProportionSize", 75, true);

        /// <summary>
        /// Gets the value of the MinimumProportion attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The minimum size a proportion can be resized to.</returns>
        public static double GetMinimumProportionSize(IControl control)
        {
            return control.GetValue(MinimumProportionSizeProperty);
        }

        /// <summary>
        /// Sets the value of the MinimumProportionSize attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The minimum size a proportion can be resized to.</param>
        public static void SetMinimumProportionSize(IControl control, double value)
        {
            control.SetValue(MinimumProportionSizeProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the thickness (height or width, depending on orientation).
        /// </summary>
        /// <value>The thickness.</value>
        public double Thickness
        {
            get => GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnDragDelta(VectorEventArgs e)
        {
            if (GetPanel() is { } panel)
            {
                SetTargetProportion(panel.Orientation == Orientation.Vertical ? e.Vector.Y : e.Vector.X);
            }
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (GetPanel() is { } panel)
            {
                if (panel.Orientation == Orientation.Vertical)
                {
                    return new Size(0, Thickness);
                }
                else
                {
                    return new Size(Thickness, 0);
                }
            }

            return new Size();
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            var panel = GetPanel();
            if (panel is null)
            {
                return;
            }

            UpdateHeightOrWidth();
        }

        private void SetTargetProportion(double dragDelta)
        {
            var target = GetTargetElement();
            var panel = GetPanel();
            if (target is null || panel is null)
            {
                return;
            }

            var children = panel.GetChildren();

            var index = children.IndexOf(this) + 1;

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

            var minProportion = GetValue(MinimumProportionSizeProperty) / (panel.Orientation == Orientation.Vertical ? panel.Bounds.Height : panel.Bounds.Width);

            if (targetElementProportion < minProportion)
            {
                dProportion = targetElementProportion - minProportion;
                neighbourProportion += dProportion;
                targetElementProportion -= dProportion;

            }
            else if (neighbourProportion < minProportion)
            {
                dProportion = neighbourProportion - minProportion;
                neighbourProportion -= dProportion;
                targetElementProportion += dProportion;
            }

            SetProportion(target, targetElementProportion);

            SetProportion(child, neighbourProportion);

            panel.InvalidateMeasure();
            panel.InvalidateArrange();
        }

        private void UpdateHeightOrWidth()
        {
            if (GetPanel() is { } panel)
            {
                if (panel.Orientation == Orientation.Vertical)
                {
                    Height = Thickness;
                    Width = double.NaN;
                    Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                    PseudoClasses.Add(":vertical");
                }
                else
                {
                    Width = Thickness;
                    Height = double.NaN;
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    PseudoClasses.Add(":horizontal");
                }
            }
        }

        private ProportionalStackPanel? GetPanel()
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

        private Control? GetTargetElement()
        {
            if (Parent is ContentPresenter presenter)
            {
                if (presenter.GetVisualParent() is not Panel panel)
                {
                    return null;
                }

                var index = panel.Children.IndexOf(Parent);
                if (index > 0 && panel.Children.Count > 0)
                {
                    if (panel.Children[index - 1] is ContentPresenter contentPresenter)
                    {
                        return contentPresenter.Child as Control;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                var panel = GetPanel();
                if (panel is { })
                {
                    var index = panel.Children.IndexOf(this);
                    if (index > 0 && panel.Children.Count > 0)
                    {
                        return panel.Children[index - 1] as Control;
                    }
                }
            }

            return null;
        }
    }
}
