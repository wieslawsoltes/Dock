// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Dock window.
    /// </summary>
    public class DockWindow : ObservableObject, IDockWindow
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _title;
        private object _context;
        private IDock _layout;
        private IDockHost _host;

        /// <inheritdoc/>
        public double X
        {
            get => _x;
            set => Update(ref _x, value);
        }

        /// <inheritdoc/>
        public double Y
        {
            get => _y;
            set => Update(ref _y, value);
        }

        /// <inheritdoc/>
        public double Width
        {
            get => _width;
            set => Update(ref _width, value);
        }

        /// <inheritdoc/>
        public double Height
        {
            get => _height;
            set => Update(ref _height, value);
        }

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set => Update(ref _title, value);
        }

        /// <inheritdoc/>
        public object Context
        {
            get => _context;
            set => Update(ref _context, value);
        }

        /// <inheritdoc/>
        public IDock Layout
        {
            get => _layout;
            set => Update(ref _layout, value);
        }

        /// <inheritdoc/>
        public IDockHost Host
        {
            get => _host;
            set => Update(ref _host, value);
        }

        /// <inheritdoc/>
        public void Present()
        {
            _host?.SetPosition(_x, _y);
            _host?.SetSize(_width, _height);
            _host?.SetTitle(_title);
            _host?.SetContext(_context);
            _host?.SetLayout(_layout);
            _host?.Present();
        }

        /// <inheritdoc/>
        public void Destroy()
        {
            _host?.GetPosition(ref _x, ref _y);
            _host?.GetSize(ref _width, ref _height);
            _host?.Destroy();
        }

        /// <summary>
        /// Check whether the <see cref="X"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeX() => true;

        /// <summary>
        /// Check whether the <see cref="Y"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeY() => true;

        /// <summary>
        /// Check whether the <see cref="Width"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeWidth() => true;

        /// <summary>
        /// Check whether the <see cref="Height"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHeight() => true;

        /// <summary>
        /// Check whether the <see cref="Title"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeTitle() => _title != null;

        /// <summary>
        /// Check whether the <see cref="Context"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeContext() => false;

        /// <summary>
        /// Check whether the <see cref="Layout"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeLayout() => _layout != null;

        /// <summary>
        /// Check whether the <see cref="Host"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHost() => false;
    }
}
