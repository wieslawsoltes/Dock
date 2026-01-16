// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Controls when document close buttons are displayed.
/// </summary>
public enum DocumentCloseButtonShowMode
{
    /// <summary>
    /// Always show close buttons.
    /// </summary>
    Always,

    /// <summary>
    /// Show close buttons only for the active document.
    /// </summary>
    Active,

    /// <summary>
    /// Never show close buttons.
    /// </summary>
    Never
}
