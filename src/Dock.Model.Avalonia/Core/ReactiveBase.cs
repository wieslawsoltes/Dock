// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Avalonia.Core;

/// <summary>
/// Reactive base class.
/// </summary>
[DataContract(IsReference = true)]
[Serializable]
public abstract class ReactiveBase : StyledElement;
