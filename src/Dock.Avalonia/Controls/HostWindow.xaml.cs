// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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
        private Button closeButton;
        private Image icon;
        private Grid leftVerticalGrip;
        private Button minimiseButton;
        
        private Button restoreButton;
        private Grid rightVerticalGrip;

        private Grid titleBar;
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

        public void AttachGrip(StripControl stripControl)
        {
            var grip = stripControl.FindControl<Grid>("PART_Grip");
            var border = stripControl.FindControl<Border>("PART_Border");            

            topHorizontalGrip = this.Find<Grid>("topHorizontalGrip");
            bottomHorizontalGrip = this.Find<Grid>("bottomHorizontalGrip");
            leftVerticalGrip = this.Find<Grid>("leftVerticalGrip");
            rightVerticalGrip = this.Find<Grid>("rightVerticalGrip");

            topLeftGrip = this.Find<Grid>("topLeftGrip");
            bottomLeftGrip = this.Find<Grid>("bottomLeftGrip");
            topRightGrip = this.Find<Grid>("topRightGrip");
            bottomRightGrip = this.Find<Grid>("bottomRightGrip");

            if (grip != null)
            {
                _titleBar = grip;

                //grip.IsVisible = false;

                //window.DataContext = grip.DataContext;
            }

            this.PseudoClasses.Set(":floating", true);
        }

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

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            mouseDown = false;
            base.OnPointerReleased(e);
        }

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
