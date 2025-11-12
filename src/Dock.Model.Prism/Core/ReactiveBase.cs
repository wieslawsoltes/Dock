// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using Prism.Mvvm;

namespace Dock.Model.Prism.Core;

/// <summary>
/// Reactive base class.
/// </summary>
[DataContract(IsReference = true)]
[Serializable]
public abstract class ReactiveBase : BindableBase;
