using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Metadata;
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
    [PseudoClasses(":toolwindow")]
    public class HostWindow : Window, IStyleable, IHostWindow
    {
        /// <summary>
        /// Use custom drag instead of BeginMoveDrag.
        /// </summary>
        public static bool s_useCustomDrag = true;

        private readonly DockManager _dockManager;
        private readonly HostWindowState _hostWindowState;
        private Control? _chromeGrip;
        private bool _mouseDown;
        private Point _startPoint;

        /// <summary>
        /// Define <see cref="IsToolWindow"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsToolWindowProperty = 
            AvaloniaProperty.Register<HostWindow, bool>(nameof(IsToolWindow));

        Type IStyleable.StyleKey => typeof(HostWindow);

        /// <summary>
        /// Gets or sets if this is the tool window.
        /// </summary>
        public bool IsToolWindow
        {
            get => GetValue(IsToolWindowProperty);
            set => SetValue(IsToolWindowProperty, value);
        }

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

            _dockManager = new DockManager();
            _hostWindowState = new HostWindowState(_dockManager, this);
#if DEBUG
            this.AttachDevTools();
#endif
            UpdatePseudoClasses(IsToolWindow);
        }

        /// <inheritdoc/>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            Window?.Factory?.HostWindows.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Window?.Factory?.HostWindows.Remove(this);

            if (Window is { })
            {
                Window.Factory?.OnWindowClosed(Window);

                if (IsTracked)
                {
                    Window?.Factory?.RemoveWindow(Window);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (Window is { })
            {
                if (Window.Factory?.OnWindowClosing(Window) == false)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (Window is { } && IsTracked)
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

        private void HostWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (Window is { } && IsTracked)
            {
                Window.Save();
            }

            _hostWindowState.Process(Position.ToPoint(1.0), EventType.Moved);
        }

        private void HostWindow_LayoutUpdated(object? sender, EventArgs e)
        {
            if (Window is { } && IsTracked)
            {
                Window.Save();
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
        /// <param name="chromeControl">The chrome control.</param>
        public void AttachGrip(ToolChromeControl chromeControl)
        {
            if (chromeControl.CloseButton is not null)
            {
                Observable.FromEventPattern(chromeControl.CloseButton, nameof(Button.Click)).Subscribe(_ =>
                {
                    Exit();
                });
            }

            _chromeGrip = chromeControl.Grip;
            ((IPseudoClasses)chromeControl.Classes).Add(":floating");
            IsToolWindow = true;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsToolWindowProperty)
            {
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<bool>());
            }
        }

        private void UpdatePseudoClasses(bool isToolWindow)
        {
            PseudoClasses.Set(":toolwindow", isToolWindow);
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_chromeGrip is { } && _chromeGrip.IsPointerOver)
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

            if (_chromeGrip is { } && _chromeGrip.IsPointerOver && _mouseDown && s_useCustomDrag)
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

        /// <inheritdoc/>
        public void Present(bool isDialog)
        {
            if (isDialog)
            {
                if (!IsVisible)
                {
                    if (Window is { })
                    {
                        Window.Factory?.OnWindowOpened(Window);
                    }

                    ShowDialog(null); // FIXME: Set correct parent window.
                }
            }
            else
            {
                if (!IsVisible)
                {
                    if (Window is { })
                    {
                        Window.Factory?.OnWindowOpened(Window);
                    }

                    Show();
                }
            }
        }

        /// <inheritdoc/>
        public void Exit()
        {
            if (Window is { })
            {
                if (Window.OnClose())
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
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
