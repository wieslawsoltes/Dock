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
            if (_titleBar != null)
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
