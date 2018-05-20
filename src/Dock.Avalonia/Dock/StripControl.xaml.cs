// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Dock
{
    /// <summary>
    /// Interaction logic for <see cref="StripControl"/> xaml.
    /// </summary>
    public class StripControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StripControl"/> class.
        /// </summary>
        public StripControl()
        {
            this.InitializeComponent();
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
