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
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.MarkupExtension;

/// <summary>
/// References named object.
/// </summary>
public class ReferenceExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceExtension"/> class.
    /// </summary>
    public ReferenceExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceExtension"/> class.
    /// </summary>
    /// <param name="name">The referenced object name.</param>
    public ReferenceExtension(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets referenced object name.
    /// </summary>
    [ConstructorArgument("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Provides a referenced object <see cref="object"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The referenced <see cref="object"/> instance.</returns>
    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(INameScope)) is INameScope nameScope && Name is { })
        {
            var element = nameScope.Find(Name);
            return element;
        }
        return default;
    }
}
