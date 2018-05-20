// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Dock
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    public class HostWindow : HostWindowBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostWindow"/> class.
        /// </summary>
        public HostWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
