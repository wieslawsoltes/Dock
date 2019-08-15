// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockTarget"/> xaml.
    /// </summary>
    public class DockTarget : TemplatedControl
    {
        private Grid _topIndicator;
        private Grid _bottomIndicator;
        private Grid _leftIndicator;
        private Grid _rightIndicator;
        private Grid _centerIndicator;

        private Control _topSelector;
        private Control _bottomSelector;
        private Control _leftSelector;
        private Control _rightSelector;
        private Control _centerSelector;

        static DockTarget()
        {
            IsHitTestVisibleProperty.OverrideDefaultValue(typeof(DockTarget), false);
        }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

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

        internal DockOperation GetDockOperation(Point point)
        {
            var result = DockOperation.Window;

            if (InvalidateIndicator(_leftSelector, _leftIndicator, point))
            {
                result = DockOperation.Left;
            }

            if (InvalidateIndicator(_rightSelector, _rightIndicator, point))
            {
                result = DockOperation.Right;
            }

            if (InvalidateIndicator(_topSelector, _topIndicator, point))
            {
                result = DockOperation.Top;
            }

            if (InvalidateIndicator(_bottomSelector, _bottomIndicator, point))
            {
                result = DockOperation.Bottom;
            }

            if (InvalidateIndicator(_centerSelector, _centerIndicator, point))
            {
                result = DockOperation.Fill;
            }

            return result;
        }

        private bool InvalidateIndicator(Control selector, Grid indicator, Point point)
        {
            bool result = false;

            if (selector != null)
            {
                var selectorPoint = selector.TranslatePoint(point, selector);
                if (selectorPoint != null && selector.InputHitTest(selectorPoint.Value) != null)
                {
                    indicator.Opacity = 0.5;
                    result = true;
                }
                else
                {
                    indicator.Opacity = 0;
                }
            }

            return result;
        }
    }
}
