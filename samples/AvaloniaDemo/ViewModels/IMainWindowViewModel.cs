// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model;

namespace AvaloniaDemo.ViewModels
{
    public interface IMainWindowViewModel
    {
        IDockFactory Factory { get; set; }
        IView Layout { get; set; }
        string CurrentView { get; set; }
    }
}
