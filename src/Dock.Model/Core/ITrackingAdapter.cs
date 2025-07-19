// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Tracking adapter contract.
/// </summary>
public interface ITrackingAdapter
{
    void GetVisibleBounds(out double x, out double y, out double width, out double height);
    void SetVisibleBounds(double x, double y, double width, double height);

    void GetPinnedBounds(out double x, out double y, out double width, out double height);
    void SetPinnedBounds(double x, double y, double width, double height);

    void GetTabBounds(out double x, out double y, out double width, out double height);
    void SetTabBounds(double x, double y, double width, double height);

    void GetPointerPosition(out double x, out double y);
    void SetPointerPosition(double x, double y);

    void GetPointerScreenPosition(out double x, out double y);
    void SetPointerScreenPosition(double x, double y);
}
