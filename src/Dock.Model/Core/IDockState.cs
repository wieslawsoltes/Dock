// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Dock state contract.
/// </summary>
public interface IDockState
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dock"></param>
    void Save(IDock dock);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dock"></param>
    void Restore(IDock dock);

    /// <summary>
    /// 
    /// </summary>
    void Reset();
}
