// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using Caliburn.Micro;

namespace Dock.Model.CaliburMicro.Core;

/// <summary>
/// Caliburn.Micro base class.
/// </summary>
[DataContract(IsReference = true)]
[Serializable]
public abstract class CaliburMicroBase : PropertyChangedBase;
