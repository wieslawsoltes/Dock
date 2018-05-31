// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    public abstract class DockBase : ViewBase, IDock
    {
        private NavigateAdapter _navigate;
        private IList<IView> _views;
        private IView _currentView;
        private IView _defaultView;
        private IList<IDockWindow> _windows;
        private string _dock;
        private IDockFactory _factory;

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigate = new NavigateAdapter(this);
        }

        /// <inheritdoc/>
        public IList<IView> Views
        {
            get => _views;
            set => Update(ref _views, value);
        }

        /// <inheritdoc/>
        public IView CurrentView
        {
            get => _currentView;
            set
            {
                Update(ref _currentView, value);
                Notify(nameof(CanGoBack));
                Notify(nameof(CanGoForward));
            }
        }

        /// <inheritdoc/>
        public IView DefaultView
        {
            get => _defaultView;
            set => Update(ref _defaultView, value);
        }

        /// <inheritdoc/>
        public bool CanGoBack => _navigate.CanGoBack;

        /// <inheritdoc/>
        public bool CanGoForward => _navigate.CanGoForward;

        /// <inheritdoc/>
        public IList<IDockWindow> Windows
        {
            get => _windows;
            set => Update(ref _windows, value);
        }

        /// <inheritdoc/>
        public string Dock
        {
            get => _dock;
            set => Update(ref _dock, value);
        }

        /// <inheritdoc/>
        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
        }

        /// <inheritdoc/>
        public virtual void GoBack()
        {
            _navigate.GoBack();
        }

        /// <inheritdoc/>
        public virtual void GoForward()
        {
            _navigate.GoForward();
        }

        /// <inheritdoc/>
        public virtual void Navigate(object root)
        {
            _navigate.Navigate(root, true);
        }

        /// <inheritdoc/>
        public virtual void ShowWindows()
        {
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Present(false);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void HideWindows()
        {
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Destroy();
                }
            }
        }

        /// <summary>
        /// Check whether the <see cref="Views"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeViews() => Views != null;

        /// <summary>
        /// Check whether the <see cref="CurrentView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentView() => CurrentView != null;

        /// <summary>
        /// Check whether the <see cref="DefaultView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultView() => DefaultView != null;

        /// <summary>
        /// Check whether the <see cref="CanGoBack"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCanGoBack() => false;

        /// <summary>
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCanGoForward() => false;

        /// <summary>
        /// Check whether the <see cref="Windows"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeWindows() => Windows != null;

        /// <summary>
        /// Check whether the <see cref="Dock"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDock() => !string.IsNullOrEmpty(Dock);

        /// <summary>
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeFactory() => false;
    }
}
