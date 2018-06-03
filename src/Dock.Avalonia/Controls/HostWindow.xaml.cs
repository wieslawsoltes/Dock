// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Reactive.Linq;
using System;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    public class HostWindow : HostWindowBase
    {
        private Control _titleBar;
        private bool mouseDown;
        private Point mouseDownPosition;

        private Grid bottomHorizontalGrip;
        private Grid bottomLeftGrip;
        private Grid bottomRightGrip;
        private Grid leftVerticalGrip;
        private Grid rightVerticalGrip;
        private Grid topHorizontalGrip;
        private Grid topLeftGrip;
        private Grid topRightGrip;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostWindow"/> class.
        /// </summary>
        public HostWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
        }

        /// <summary>
        /// Attaches grip to chrome.
        /// </summary>
        /// <param name="chrome">The chrome control.</param>
        public void AttachGrip(DockToolChrome chrome)
        {
            topHorizontalGrip = this.Find<Grid>("topHorizontalGrip");
            bottomHorizontalGrip = this.Find<Grid>("bottomHorizontalGrip");
            leftVerticalGrip = this.Find<Grid>("leftVerticalGrip");
            rightVerticalGrip = this.Find<Grid>("rightVerticalGrip");

            topLeftGrip = this.Find<Grid>("topLeftGrip");
            bottomLeftGrip = this.Find<Grid>("bottomLeftGrip");
            topRightGrip = this.Find<Grid>("topRightGrip");
            bottomRightGrip = this.Find<Grid>("bottomRightGrip");

            Observable.FromEventPattern(chrome.CloseButton, nameof(Button.Click)).Subscribe(o =>
            {
                Close();
            });

            //Observable.FromEventPattern(chrome.MinimiseButton, nameof(Button.Click)).Subscribe(o =>
            //{
            //    WindowState = WindowState.Minimized;
            //});

            _titleBar = chrome.Grip;

            this.PseudoClasses.Set(":floating", true);
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
            else if (_titleBar != null)
            {
                if (_titleBar.IsPointerOver)
                {
                    mouseDown = true;
                    mouseDownPosition = e.GetPosition(this);
                }
                else
                {
                    mouseDown = false;
                }
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
            if (_titleBar != null)
            {
                if (_titleBar.IsPointerOver && mouseDown)
                {
                    //if (mouseDownPosition.DistanceTo(e.GetPosition(this)) > 12)
                    {
                        WindowState = WindowState.Normal;
                        BeginMoveDrag();
                        mouseDown = false;
                    }
                }
            }

            base.OnPointerMoved(e);
        }

    }
}
