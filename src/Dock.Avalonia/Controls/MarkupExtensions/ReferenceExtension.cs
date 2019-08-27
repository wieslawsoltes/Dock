// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls
{
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
        public string Name { get; set; }

        /// <summary>
        /// Provides a referenced object <see cref="object"/> instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The referenced <see cref="object"/> instance.</returns>
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var nameScope = serviceProvider.GetService(typeof(INameScope)) as INameScope;
            var element = nameScope.Find(Name);
            return element;
        }
    }
}
