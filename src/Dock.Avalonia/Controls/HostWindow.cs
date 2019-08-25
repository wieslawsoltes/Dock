// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    public class HostWindow : Window, IStyleable, IHostWindow
    {
        private bool _pointerPressed = false;
        private Point _pointerPressedPoint = default;
        private Point _targetPoint = default;
        private IVisual _targetDockControl = null;

        private Control _titleBar;
        private Grid _bottomHorizontalGrip;
        private Grid _bottomLeftGrip;
        private Grid _bottomRightGrip;
        private Grid _leftVerticalGrip;
        private Grid _rightVerticalGrip;
        private Grid _topHorizontalGrip;
        private Grid _topLeftGrip;
        private Grid _topRightGrip;
        private Button _closeButton;
        private Button _minimiseButton;
        private Button _restoreButton;
        private Image _icon;
        private bool _mouseDown;
        private Point _mouseDownPosition;

        /// <summary>
        /// Defines the <see cref="IsChromeVisible"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsChromeVisibleProperty =
            AvaloniaProperty.Register<HostWindow, bool>(nameof(IsChromeVisible), true);

        /// <summary>
        /// Defines the <see cref="TitleBarContent"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<Control> TitleBarContentProperty =
            AvaloniaProperty.Register<HostWindow, Control>(nameof(TitleBarContent));

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

        Type IStyleable.StyleKey => typeof(HostWindow);

        /// <inheritdoc/>
        public bool IsTracked { get; set; }

        /// <inheritdoc/>
        public IDockWindow Window { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="HostWindow"/> class.
        /// </summary>
        public HostWindow()
        {
            this.AttachDevTools(new KeyGesture(Key.F12, InputModifiers.Control));

            AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

            PositionChanged += HostWindow_PositionChanged;
            LayoutUpdated += HostWindow_LayoutUpdated;
            Closing += HostWindow_Closing;
        }

        private void HostWindow_PositionChanged(object sender, PixelPointEventArgs e)
        {
            if (Window != null && IsTracked == true)
            {
                Window.Save();
            }

            DragPositionChanged();
        }

        private void HostWindow_LayoutUpdated(object sender, EventArgs e)
        {
            if (Window != null && IsTracked == true)
            {
                Window.Save();
            }
        }

        private void HostWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Window != null && IsTracked == true)
            {
                Window.Save();

                if (Window.Layout is IDock root)
                {
                    root.Close();
                }
            }
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            _pointerPressed = true;
            _pointerPressedPoint = e.GetPosition(this);
            _targetPoint = default;
            _targetDockControl = null;
            // TODO:
            Debug.WriteLine($"{nameof(HostWindow)} {nameof(Pressed)} {e.GetPosition(this)}");
        }

        private void Released(object sender, PointerReleasedEventArgs e)
        {
            _pointerPressed = false;
            _pointerPressedPoint = default;
            _targetPoint = default;
            _targetDockControl = null;
            // TODO:
            Debug.WriteLine($"{nameof(HostWindow)} {nameof(Released)} {e.GetPosition(this)}");
        }

        private void Moved(object sender, PointerEventArgs e)
        {
        }

        private void CaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            _pointerPressed = false;
            _pointerPressedPoint = default;
            _targetPoint = default;
            _targetDockControl = null;
            // TODO:
            Debug.WriteLine($"{nameof(HostWindow)} {nameof(CaptureLost)}");
        }

        private void DragPositionChanged()
        {
            if (_pointerPressed == true)
            {
                foreach (var visual in DockControl.s_dockControls)
                {
                    if (visual is DockControl dockControl)
                    {
                        if (dockControl.Layout != Window.Layout)
                        {
                            var position = this.Position.ToPoint(1.0) + _pointerPressedPoint;
                            var screenPoint = new PixelPoint((int)position.X, (int)position.Y);
                            var dockControlPoint = dockControl.PointToClient(screenPoint);
                            if (dockControlPoint == null)
                            {
                                continue;
                            }

                            var dropControl = dockControl._dockControlState.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                            if (dropControl != null)
                            {
                                _targetPoint = dockControlPoint;
                                _targetDockControl = dropControl;
                                Debug.WriteLine($"Window Drop : {dockControlPoint} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attaches grip to chrome.
        /// </summary>
        /// <param name="chrome">The chrome control.</param>
        public void AttachGrip(DockToolChrome chrome)
        {
            Observable.FromEventPattern(chrome.CloseButton, nameof(Button.Click)).Subscribe(o =>
            {
                Close();
            });

            //Observable.FromEventPattern(chrome.MinimiseButton, nameof(Button.Click)).Subscribe(o =>
            //{
            //    WindowState = WindowState.Minimized;
            //});

            _titleBar = chrome.Grip;

            ((IPseudoClasses)chrome.Classes).Add(":floating");
            this.PseudoClasses.Set(":toolwindow", true);
        }

        /// <summary>
        /// Toggles window state.
        /// </summary>
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
                BeginResizeDrag(WindowEdge.North);
            }
            else if (_bottomHorizontalGrip != null && _bottomHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.South);
            }
            else if (_leftVerticalGrip != null && _leftVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.West);
            }
            else if (_rightVerticalGrip != null && _rightVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.East);
            }
            else if (_topLeftGrip != null && _topLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthWest);
            }
            else if (_bottomLeftGrip != null && _bottomLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthWest);
            }
            else if (_topRightGrip != null && _topRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthEast);
            }
            else if (_bottomRightGrip != null && _bottomRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthEast);
            }
            else if (_titleBar != null && _titleBar.IsPointerOver)
            {
                _mouseDown = true;
                _mouseDownPosition = e.GetPosition(this);
            }
            else
            {
                _mouseDown = false;
            }

            base.OnPointerPressed(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _mouseDown = false;
            base.OnPointerReleased(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_titleBar != null && _titleBar.IsPointerOver && _mouseDown)
            {
                WindowState = WindowState.Normal;
                BeginMoveDrag();
                _mouseDown = false;
            }
            base.OnPointerMoved(e);
        }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            _titleBar = e.NameScope.Find<Control>("PART_TitleBar");
            _minimiseButton = e.NameScope.Find<Button>("PART_MinimiseButton");
            _restoreButton = e.NameScope.Find<Button>("PART_RestoreButton");
            _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
            _icon = e.NameScope.Find<Image>("PART_Icon");

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

        /// <inheritdoc/>
        public void Present(bool isDialog)
        {
            if (isDialog)
            {
                if (!this.IsVisible)
                {
                    this.ShowDialog(null); // FIXME: Set correct parent window.
                }
            }
            else
            {
                if (!this.IsVisible)
                {
                    this.Show();
                }
            }
        }

        /// <inheritdoc/>
        public void Exit()
        {
            this.Close();
        }

        /// <inheritdoc/>
        public void SetPosition(double x, double y)
        {
            if (!double.IsNaN(x) && !double.IsNaN(y))
            {
                Position = new PixelPoint((int)x, (int)y);
            }
        }

        /// <inheritdoc/>
        public void GetPosition(out double x, out double y)
        {
            x = this.Position.X;
            y = this.Position.Y;
        }

        /// <inheritdoc/>
        public void SetSize(double width, double height)
        {
            if (!double.IsNaN(width))
            {
                this.Width = width;
            }

            if (!double.IsNaN(height))
            {
                this.Height = height;
            }
        }

        /// <inheritdoc/>
        public void GetSize(out double width, out double height)
        {
            width = this.Width;
            height = this.Height;
        }

        /// <inheritdoc/>
        public void SetTopmost(bool topmost)
        {
            this.Topmost = topmost;
        }

        /// <inheritdoc/>
        public void SetTitle(string title)
        {
            this.Title = title;
        }

        /// <inheritdoc/>
        public void SetLayout(IDock layout)
        {
            this.DataContext = layout;
        }
    }
}
