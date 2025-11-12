// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core;

/// <summary>
/// Reactive base class.
/// </summary>
[DataContract(IsReference = true)]
[Serializable]
[Reactive]
public abstract class ReactiveBase : ReactiveObject;
