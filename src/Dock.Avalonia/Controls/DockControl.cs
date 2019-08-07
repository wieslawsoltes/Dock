// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockControl, IDock>(nameof(Layout));

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        /// <value>The layout.</value>
        public IDock Layout
        {
            get { return GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }
    }
}
