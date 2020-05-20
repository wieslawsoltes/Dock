// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockTarget"/> xaml.
    /// </summary>
    public class DockTarget : TemplatedControl
    {
        private Grid? _topIndicator;
        private Grid? _bottomIndicator;
        private Grid? _leftIndicator;
        private Grid? _rightIndicator;
        private Grid? _centerIndicator;

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

            _topIndicator = e.NameScope.Find<Grid>("PART_TopIndicator");
            _bottomIndicator = e.NameScope.Find<Grid>("PART_BottomIndicator");
            _leftIndicator = e.NameScope.Find<Grid>("PART_LeftIndicator");
            _rightIndicator = e.NameScope.Find<Grid>("PART_RightIndicator");
            _centerIndicator = e.NameScope.Find<Grid>("PART_CenterIndicator");

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

        private bool InvalidateIndicator(Control? selector, Grid? indicator, Point point, IVisual relativeTo, DockOperation operation, DragAction dragAction, Func<Point, DockOperation, DragAction, IVisual, bool> validate)
        {
            if (selector != null && indicator != null)
            {
                var selectorPoint = relativeTo.TranslatePoint(point, selector);
                if (selectorPoint != null && selector.InputHitTest(selectorPoint.Value) != null && validate(point, operation, dragAction, relativeTo) == true)
                {
                    indicator.Opacity = 0.5;
                    return true;
                }
                else
                {
                    indicator.Opacity = 0;
                    return false;
                }
            }
            return false;
        }
    }
}
