/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.MarkupExtension;

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
        if (serviceProvider.GetService(typeof(IUriContext)) is IUriContext uriContext && Source is not null)
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
