// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Globalization;
using System.Runtime.Serialization;

namespace Dock.Model.Core;

/// <summary>
/// Rectangle structure.
/// </summary>
[DataContract]
public struct DockRect
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    [DataMember(Order = 1)]
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    [DataMember(Order = 2)]
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    [DataMember(Order = 3)]
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    [DataMember(Order = 4)]
    public double Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockRect"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public DockRect(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Returns the string representation of the rectangle.
    /// </summary>
    /// <returns>The string representation of the rectangle.</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", X, Y, Width, Height);
    }
}
