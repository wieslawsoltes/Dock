// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo
{
    /// <summary>
    /// Interaction logic for <see cref="NavigationControl"/> xaml.
    /// </summary>
    public class NavigationControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationControl"/> class.
        /// </summary>
        public NavigationControl()
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
