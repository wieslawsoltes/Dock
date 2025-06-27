// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Avalonia.Controls.Recycling.Model;

/// <summary>
/// 
/// </summary>
public interface IControlRecyclingIdProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string? GetControlRecyclingId();
}
