// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model;

namespace AvaloniaDemo
{
    public class MainWindowViewModel : ObservableObject
    {
        private IDock _layout;

        public IDock Layout
        {
            get => _layout;
            set => Update(ref _layout, value);
        }
    }
}
