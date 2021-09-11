using System;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.MarkupExtension
{
    /// <summary>
    /// Loads XAML <see cref="object"/> from a URI.
    /// </summary>
    public class LoadExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadExtension"/> class.
        /// </summary>
        public LoadExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadExtension"/> class.
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
        public object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IUriContext)) is IUriContext uriContext)
            {
                var baseUri = uriContext.BaseUri;
                var obj = AvaloniaXamlLoader.Load(Source, baseUri);
                return obj;
            }
            return default;
        }

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        [ConstructorArgument("source")]
        public Uri? Source { get; set; }
    }
}
