// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model;
//using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        private IDockFactory _factory;
        private IView _layout;
        private string _currentView;

        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IView Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
    }
}
