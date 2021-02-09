using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockTarget"/> xaml.
    /// </summary>
    public class DockTarget : TemplatedControl
    {
        private Panel? _topIndicator;
        private Panel? _bottomIndicator;
        private Panel? _leftIndicator;
        private Panel? _rightIndicator;
        private Panel? _centerIndicator;
        private Control? _topSelector;
        private Control? _bottomSelector;
        private Control? _leftSelector;
        private Control? _rightSelector;
        private Control? _centerSelector;

        static DockTarget()
        {
            IsHitTestVisibleProperty.OverrideDefaultValue(typeof(DockTarget), false);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _topIndicator = e.NameScope.Find<Panel>("PART_TopIndicator");
            _bottomIndicator = e.NameScope.Find<Panel>("PART_BottomIndicator");
            _leftIndicator = e.NameScope.Find<Panel>("PART_LeftIndicator");
            _rightIndicator = e.NameScope.Find<Panel>("PART_RightIndicator");
            _centerIndicator = e.NameScope.Find<Panel>("PART_CenterIndicator");

            _topSelector = e.NameScope.Find<Control>("PART_TopSelector");
            _bottomSelector = e.NameScope.Find<Control>("PART_BottomSelector");
            _leftSelector = e.NameScope.Find<Control>("PART_LeftSelector");
            _rightSelector = e.NameScope.Find<Control>("PART_RightSelector");
            _centerSelector = e.NameScope.Find<Control>("PART_CenterSelector");
        }

        internal DockOperation GetDockOperation(Point point, IVisual relativeTo, DragAction dragAction, Func<Point, DockOperation, DragAction, IVisual, bool> validate)
        {
            var result = DockOperation.Window;

            if (InvalidateIndicator(_leftSelector, _leftIndicator, point, relativeTo, DockOperation.Left, dragAction, validate))
            {
                result = DockOperation.Left;
            }

            if (InvalidateIndicator(_rightSelector, _rightIndicator, point, relativeTo, DockOperation.Right, dragAction, validate))
            {
                result = DockOperation.Right;
            }

            if (InvalidateIndicator(_topSelector, _topIndicator, point, relativeTo, DockOperation.Top, dragAction, validate))
            {
                result = DockOperation.Top;
            }

            if (InvalidateIndicator(_bottomSelector, _bottomIndicator, point, relativeTo, DockOperation.Bottom, dragAction, validate))
            {
                result = DockOperation.Bottom;
            }

            if (InvalidateIndicator(_centerSelector, _centerIndicator, point, relativeTo, DockOperation.Fill, dragAction, validate))
            {
                result = DockOperation.Fill;
            }

            return result;
        }

        private bool InvalidateIndicator(Control? selector, Panel? indicator, Point point, IVisual relativeTo, DockOperation operation, DragAction dragAction, Func<Point, DockOperation, DragAction, IVisual, bool> validate)
        {
            if (selector is null || indicator is null)
            {
                return false;
            }

            var selectorPoint = relativeTo.TranslatePoint(point, selector);
            if (selectorPoint is { } && selector.InputHitTest(selectorPoint.Value) is { } && validate(point, operation, dragAction, relativeTo))
            {
                indicator.Opacity = 0.5;
                return true;
            }

            indicator.Opacity = 0;
            return false;
        }
    }
}
