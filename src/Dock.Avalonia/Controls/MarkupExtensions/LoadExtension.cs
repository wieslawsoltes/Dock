// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Loads XAML <see cref="object"/> from a URI.
    /// </summary>
    public class LoadExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutIncludeExtension"/> class.
        /// </summary>
        public LoadExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutIncludeExtension"/> class.
        /// </summary>
        /// <param name="source">The source uri.</param>
        public LoadExtension(Uri source)
        {
            Source = source;
        }

        /// <summary>
        /// Provides a loaded <see cref="object"/> instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The loaded <see cref="object"/> instance.</returns>
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IUriContext)) is IUriContext uriContext)
            {
                var baseUri = uriContext.BaseUri;
                var loader = new AvaloniaXamlLoader();
                var obj = loader.Load(Source, baseUri);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        [ConstructorArgument("source")]
        public Uri Source { get; set; }
    }
}
