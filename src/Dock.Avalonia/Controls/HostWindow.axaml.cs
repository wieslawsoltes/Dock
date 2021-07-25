using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    [PseudoClasses(":toolwindow", ":dragging")]
    public class HostWindow : Window, IStyleable, IHostWindow
    {
        private readonly DockManager _dockManager;
        private readonly HostWindowState _hostWindowState;
        private Control? _chromeGrip;
        private HostWindowTitleBar? _hostWindowTitleBar;
        private bool _mouseDown;

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
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _hostWindowTitleBar = e.NameScope.Find<HostWindowTitleBar>("PART_TitleBar");
            if (_hostWindowTitleBar is { })
            {
                _hostWindowTitleBar.ApplyTemplate();

                if (_hostWindowTitleBar.BackgroundControl is { })
                {
                    _hostWindowTitleBar.BackgroundControl.PointerPressed += (_, args) =>
                    {
                        MoveDrag(args);
                    };
                }
            }
        }

        private void MoveDrag(PointerPressedEventArgs e)
        {
            if (Window?.Factory?.OnWindowMoveDragBegin(Window) != true)
            {
                return;
            }

            _mouseDown = true;
            _hostWindowState.Process(e.GetPosition(this), EventType.Pressed);

            PseudoClasses.Set(":dragging", true);
            BeginMoveDrag(e);
            PseudoClasses.Set(":dragging", false);

            Window?.Factory?.OnWindowMoveDragEnd(Window);
            _hostWindowState.Process(e.GetPosition(this), EventType.Released);
            _mouseDown = false;
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_chromeGrip is { } && _chromeGrip.IsPointerOver)
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    MoveDrag(e);
                }
            }
        }

        private void HostWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (Window is { } && IsTracked)
            {
                Window.Save();

                if ((_chromeGrip is { } && _chromeGrip.IsPointerOver)
                    || (_hostWindowTitleBar?.BackgroundControl is { } && (_hostWindowTitleBar?.BackgroundControl?.IsPointerOver ?? false))
                    && _mouseDown)
                {
                    Window.Factory?.OnWindowMoveDrag(Window);
                    _hostWindowState.Process(Position.ToPoint(1.0), EventType.Moved);
                }
            }
        }

        private void HostWindow_LayoutUpdated(object? sender, EventArgs e)
        {
            if (Window is { } && IsTracked)
            {
                Window.Save();
            }
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
