// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="MetroWindow"/> xaml.
    /// </summary>
    public class MetroWindow : Window, IStyleable
    {
        private Grid titleBar;

        private Grid bottomHorizontalGrip;
        private Grid bottomLeftGrip;
        private Grid bottomRightGrip;
        private Grid leftVerticalGrip;
        private Grid rightVerticalGrip;
        private Grid topHorizontalGrip;
        private Grid topLeftGrip;
        private Grid topRightGrip;

        private Button closeButton;
        private Button minimiseButton;
        private Button restoreButton;

        private Image icon;

        private bool mouseDown;
        private Point mouseDownPosition;

        /// <summary>
        /// Defines the <see cref="HideChrome"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> HideChromeProperty =
            AvaloniaProperty.Register<MetroWindow, bool>(nameof(HideChrome));

        /// <summary>
        /// Defines the <see cref="TitleBarContent"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<Control> TitleBarContentProperty =
            AvaloniaProperty.Register<MetroWindow, Control>(nameof(TitleBarContent));

        /// <summary>
        ///  Gets or sets the flag indicating whether to hide chrome.
        /// </summary>
        public bool HideChrome
        {
            get => GetValue(HideChromeProperty);
            set => SetValue(HideChromeProperty, value);
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
            if (topHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.North);
            }
            else if (bottomHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.South);
            }
            else if (leftVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.West);
            }
            else if (rightVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.East);
            }
            else if (topLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthWest);
            }
            else if (bottomLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthWest);
            }
            else if (topRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthEast);
            }
            else if (bottomRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthEast);
            }
            else if (titleBar.IsPointerOver)
            {
                mouseDown = true;
                mouseDownPosition = e.GetPosition(this);
            }
            else
            {
                mouseDown = false;
            }

            base.OnPointerPressed(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            mouseDown = false;
            base.OnPointerReleased(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (titleBar.IsPointerOver && mouseDown)
            {
                WindowState = WindowState.Normal;
                BeginMoveDrag();
                mouseDown = false;
            }

            base.OnPointerMoved(e);
        }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            titleBar = e.NameScope.Find<Grid>("titlebar");
            minimiseButton = e.NameScope.Find<Button>("minimiseButton");
            restoreButton = e.NameScope.Find<Button>("restoreButton");
            closeButton = e.NameScope.Find<Button>("closeButton");
            icon = e.NameScope.Find<Image>("icon");

            topHorizontalGrip = e.NameScope.Find<Grid>("topHorizontalGrip");
            bottomHorizontalGrip = e.NameScope.Find<Grid>("bottomHorizontalGrip");
            leftVerticalGrip = e.NameScope.Find<Grid>("leftVerticalGrip");
            rightVerticalGrip = e.NameScope.Find<Grid>("rightVerticalGrip");

            topLeftGrip = e.NameScope.Find<Grid>("topLeftGrip");
            bottomLeftGrip = e.NameScope.Find<Grid>("bottomLeftGrip");
            topRightGrip = e.NameScope.Find<Grid>("topRightGrip");
            bottomRightGrip = e.NameScope.Find<Grid>("bottomRightGrip");

            minimiseButton.Click += (sender, ee) => { WindowState = WindowState.Minimized; };

            restoreButton.Click += (sender, ee) => { ToggleWindowState(); };

            titleBar.DoubleTapped += (sender, ee) => { ToggleWindowState(); };

            closeButton.Click += (sender, ee) => { Close(); };

            //icon.DoubleTapped += (sender, ee) => { Close(); };
        }
    }
}
