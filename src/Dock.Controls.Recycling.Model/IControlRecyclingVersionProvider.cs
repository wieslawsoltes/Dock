// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Avalonia.Controls.Recycling.Model;

/// <summary>
/// Optionally supplies content revisions to a recycling cache. A changed revision
/// invalidates controls created by the same source. A replacement source sharing
/// an explicit recycling ID can reuse the previous control.
/// </summary>
public interface IControlRecyclingVersionProvider
{
    /// <summary>
    /// Gets the revision of the content currently used to build controls.
    /// </summary>
    ControlRecyclingVersion GetControlRecyclingVersion();
}
