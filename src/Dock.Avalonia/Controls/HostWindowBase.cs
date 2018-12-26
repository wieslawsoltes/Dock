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
            PositionChanged += (sender, e) =>
            {
#if DEBUG
                Console.WriteLine($"HostWindowBase: PositionChanged");
#endif
                if (Window != null && IsTracked == true)
                {
                    Window.Save();
                }
            };

            LayoutUpdated += (sender, e) =>
            {
#if DEBUG
                Console.WriteLine($"HostWindowBase: LayoutUpdated");
#endif
                if (Window != null && IsTracked == true)
                {
                    Window.Save();
                }
            };

            Closing += (sender, e) =>
            {
#if DEBUG
                Console.WriteLine($"HostWindowBase: Closing");
#endif
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
            if (x != double.NaN && y != double.NaN)
            {
                Position = new Point(x, y);
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
#if DEBUG
            Console.WriteLine($"HostWindowBase: SetSize {width}x{height}");
#endif
            if (width != double.NaN)
            {
                this.Width = width;
            }

            if (height != double.NaN)
            {
                this.Height = height;
            }
        }

        /// <inheritdoc/>
        public void GetSize(out double width, out double height)
        {
            width = this.Width;
            height = this.Height;
#if DEBUG
            Console.WriteLine($"HostWindowBase: GetSize {width}x{height}");
#endif
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
            var dock = this.FindControl<IControl>("dock");
            if (dock != null)
            {
                dock.DataContext = layout;
            }
        }
    }
}
