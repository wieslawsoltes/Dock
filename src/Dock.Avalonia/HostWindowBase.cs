// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Host window base class.
    /// </summary>
    public abstract class HostWindowBase : Window, IDockHost
    {
        /// <inheritdoc/>
        public void Present()
        {
            this.Show();
        }

        /// <inheritdoc/>
        public void Destroy()
        {
            this.Hide();
        }

        /// <inheritdoc/>
        public void Exit()
        {
            this.Close();
        }

        /// <inheritdoc/>
        public void SetPosition(double x, double y)
        {
            Position = new Point(x, y);
        }

        /// <inheritdoc/>
        public void GetPosition(ref double x, ref double y)
        {
            x = this.Position.X;
            y = this.Position.Y;
        }

        /// <inheritdoc/>
        public void SetSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <inheritdoc/>
        public void GetSize(ref double width, ref double height)
        {
            width = this.Width;
            height = this.Height;
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
