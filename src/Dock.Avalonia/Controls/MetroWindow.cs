using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="MetroWindow"/> xaml.
    /// </summary>
    public class MetroWindow : Window, IStyleable
    {
        private Grid? _titleBar;
        private Grid? _bottomHorizontalGrip;
        private Grid? _bottomLeftGrip;
        private Grid? _bottomRightGrip;
        private Grid? _leftVerticalGrip;
        private Grid? _rightVerticalGrip;
        private Grid? _topHorizontalGrip;
        private Grid? _topLeftGrip;
        private Grid? _topRightGrip;
        private Button? _closeButton;
        private Button? _minimiseButton;
        private Button? _restoreButton;
        //private Image? _icon;
        //private bool _mouseDown;

        /// <summary>
        /// Defines the <see cref="IsChromeVisible"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsChromeVisibleProperty =
            AvaloniaProperty.Register<MetroWindow, bool>(nameof(IsChromeVisible), true);

        /// <summary>
        /// Defines the <see cref="TitleBarContent"/> property.
        /// </summary>
        public static readonly StyledProperty<Control> TitleBarContentProperty =
            AvaloniaProperty.Register<MetroWindow, Control>(nameof(TitleBarContent));

        /// <summary>
        ///  Gets or sets the flag indicating whether chrome is visible.
        /// </summary>
        public bool IsChromeVisible
        {
            get => GetValue(IsChromeVisibleProperty);
            set => SetValue(IsChromeVisibleProperty, value);
        }

        /// <summary>
        ///  Gets or sets the title bar content control.
        /// </summary>
        public Control TitleBarContent
        {
            get => GetValue(TitleBarContentProperty);
            set => SetValue(TitleBarContentProperty, value);
        }

        Type IStyleable.StyleKey => typeof(MetroWindow);

        private void ToggleWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_topHorizontalGrip != null && _topHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.North, e);
            }
            else if (_bottomHorizontalGrip != null && _bottomHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.South, e);
            }
            else if (_leftVerticalGrip != null && _leftVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.West, e);
            }
            else if (_rightVerticalGrip != null && _rightVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.East, e);
            }
            else if (_topLeftGrip != null && _topLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthWest, e);
            }
            else if (_bottomLeftGrip != null && _bottomLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthWest, e);
            }
            else if (_topRightGrip != null && _topRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthEast, e);
            }
            else if (_bottomRightGrip != null && _bottomRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthEast, e);
            }
            else if (_titleBar != null && _titleBar.IsPointerOver)
            {
                //_mouseDown = true;

                if(e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                    //_mouseDown = false;
                }
            }
            else
            {
                //_mouseDown = false;
            }

            base.OnPointerPressed(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            //_mouseDown = false;
            base.OnPointerReleased(e);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _titleBar = e.NameScope.Find<Grid>("PART_TitleBar");
            _minimiseButton = e.NameScope.Find<Button>("PART_MinimiseButton");
            _restoreButton = e.NameScope.Find<Button>("PART_RestoreButton");
            _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
            //_icon = e.NameScope.Find<Image>("PART_Icon");

            _topHorizontalGrip = e.NameScope.Find<Grid>("PART_TopHorizontalGrip");
            _bottomHorizontalGrip = e.NameScope.Find<Grid>("PART_BottomHorizontalGrip");
            _leftVerticalGrip = e.NameScope.Find<Grid>("PART_LeftVerticalGrip");
            _rightVerticalGrip = e.NameScope.Find<Grid>("PART_RightVerticalGrip");

            _topLeftGrip = e.NameScope.Find<Grid>("PART_TopLeftGrip");
            _bottomLeftGrip = e.NameScope.Find<Grid>("PART_BottomLeftGrip");
            _topRightGrip = e.NameScope.Find<Grid>("PART_TopRightGrip");
            _bottomRightGrip = e.NameScope.Find<Grid>("PART_BottomRightGrip");

            if (_minimiseButton != null)
            {
                _minimiseButton.Click += (sender, ee) => { WindowState = WindowState.Minimized; };

            }

            if (_restoreButton != null)
            {
                _restoreButton.Click += (sender, ee) => { ToggleWindowState(); };
            }

            if (_titleBar != null)
            {
                _titleBar.DoubleTapped += (sender, ee) => { ToggleWindowState(); };
            }

            if (_closeButton != null)
            {
                _closeButton.Click += (sender, ee) => { Close(); };
            }

            //if (_icon != null)
            //{
            //    _icon.DoubleTapped += (sender, ee) => { Close(); };
            //}
        }
    }
}
