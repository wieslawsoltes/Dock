using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    public class HostWindow : Window, IStyleable, IHostWindow
    {
        private static bool s_useCustomDrag = true;
        private static readonly List<IHostWindow> s_hostWindows = new();
        private readonly DockManager _dockManager;
        private readonly HostWindowState _hostWindowState;
        private Control? _chromeGrip;
        private bool _mouseDown;
        private Point _startPoint;

        Type IStyleable.StyleKey => typeof(HostWindow);

        /// <inheritdoc/>
        public IList<IHostWindow> HostWindows => s_hostWindows;

        /// <inheritdoc/>
        public IDockManager DockManager => _dockManager;

        /// <inheritdoc/>
        public IHostWindowState HostWindowState => _hostWindowState;

        /// <inheritdoc/>
        public bool IsTracked { get; set; }

        /// <inheritdoc/>
        public IDockWindow? Window { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="HostWindow"/> class.
        /// </summary>
        public HostWindow()
        {
            AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(PointerReleasedEvent, Released, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(PointerMovedEvent, Moved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AddHandler(PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

            PositionChanged += HostWindow_PositionChanged;
            LayoutUpdated += HostWindow_LayoutUpdated;
            Closing += HostWindow_Closing;

            _dockManager = new DockManager();
            _hostWindowState = new HostWindowState(_dockManager, this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        /// <inheritdoc/>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            s_hostWindows.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            s_hostWindows.Remove(this);
        }

        private void HostWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (Window != null && IsTracked)
            {
                Window.Save();
            }

            _hostWindowState.Process(Position.ToPoint(1.0), EventType.Moved);
        }

        private void HostWindow_LayoutUpdated(object? sender, EventArgs e)
        {
            if (Window != null && IsTracked)
            {
                Window.Save();
            }
        }

        private void HostWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (Window != null && IsTracked)
            {
                Window.Save();

                if (Window.Layout is IDock root)
                {
                    if (root.Close.CanExecute(null))
                    {
                        root.Close.Execute(null);
                    }
                }
            }
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            _hostWindowState.Process(e.GetPosition(this), EventType.Pressed);
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            _hostWindowState.Process(e.GetPosition(this), EventType.Released);
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            // Using PositionChanged event instead of PointerMoved event.
        }

        private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _hostWindowState.Process(new Point(), EventType.CaptureLost);
        }

        /// <summary>
        /// Attaches grip to chrome.
        /// </summary>
        /// <param name="chrome">The chrome control.</param>
        public void AttachGrip(DockToolChrome chrome)
        {
            if (chrome.CloseButton is not null)
            {
                Observable.FromEventPattern(chrome.CloseButton, nameof(Button.Click)).Subscribe(_ =>
                {
                    Close();
                });
            }

            _chromeGrip = chrome.Grip;

            ((IPseudoClasses)chrome.Classes).Add(":floating");
            PseudoClasses.Set(":toolwindow", true);
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_chromeGrip != null && _chromeGrip.IsPointerOver)
            {
                _mouseDown = true;
                _startPoint = e.GetPosition(this);

                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !s_useCustomDrag)
                {
                    BeginMoveDrag(e);
                    _mouseDown = false;
                }
            }
            else
            {
                _mouseDown = false;
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            _mouseDown = false;
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (_chromeGrip != null && _chromeGrip.IsPointerOver && _mouseDown)
            {
                if (s_useCustomDrag)
                {
                    // Using custom method because BeginMoveDrag is releasing pointer capture on Windows.
                    var point = e.GetPosition(this);
                    var delta = point - _startPoint;
                    var x = Position.X + delta.X;
                    var y = Position.Y + delta.Y;
                    Position = Position.WithX((int)x).WithY((int)y);
                    _startPoint = new Point(point.X - delta.X, point.Y - delta.Y);
                }
            }
        }

        /// <inheritdoc/>
        public void Present(bool isDialog)
        {
            if (isDialog)
            {
                if (!IsVisible)
                {
                    ShowDialog(null); // FIXME: Set correct parent window.
                }
            }
            else
            {
                if (!IsVisible)
                {
                    Show();
                }
            }
        }

        /// <inheritdoc/>
        public void Exit()
        {
            Close();
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
            x = Position.X;
            y = Position.Y;
        }

        /// <inheritdoc/>
        public void SetSize(double width, double height)
        {
            if (!double.IsNaN(width))
            {
                Width = width;
            }

            if (!double.IsNaN(height))
            {
                Height = height;
            }
        }

        /// <inheritdoc/>
        public void GetSize(out double width, out double height)
        {
            width = Width;
            height = Height;
        }

        /// <inheritdoc/>
        public void SetTopmost(bool topmost)
        {
            Topmost = topmost;
        }

        /// <inheritdoc/>
        public void SetTitle(string title)
        {
            Title = title;
        }

        /// <inheritdoc/>
        public void SetLayout(IDock layout)
        {
            DataContext = layout;
        }
    }
}
