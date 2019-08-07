// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia;
using Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Host window base class.
    /// </summary>
    public abstract class HostWindowBase : MetroWindow, IDockHost
    {
        /// <inheritdoc/>
        public bool IsTracked { get; set; }

        /// <inheritdoc/>
        public IDockWindow Window { get; set; }

        /// <summary>
        /// Initializes new instance of the <see cref="HostWindowBase"/> class.
        /// </summary>
        public HostWindowBase()
        {
            this.AttachDevTools();

            PositionChanged += (sender, e) =>
            {
                Logger.Log($"HostWindowBase: PositionChanged");
                if (Window != null && IsTracked == true)
                {
                    Window.Save();
                }
            };

            LayoutUpdated += (sender, e) =>
            {
                Logger.Log($"HostWindowBase: LayoutUpdated");
                if (Window != null && IsTracked == true)
                {
                    Window.Save();
                }
            };

            Closing += (sender, e) =>
            {
                Logger.Log($"HostWindowBase: Closing");
                if (Window != null && IsTracked == true)
                {
                    Window.Save();

                    if (Window.Layout is IDock root)
                    {
                        root.Close();
                    }
                }
            };
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
            Logger.Log($"HostWindowBase: SetSize {width}x{height}");
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
            Logger.Log($"HostWindowBase: GetSize {width}x{height}");
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
        public void SetContext(object context)
        {
            this.DataContext = context;
        }

        /// <inheritdoc/>
        public void SetLayout(IDock layout)
        {
            this.DataContext = layout;
        }
    }
}
