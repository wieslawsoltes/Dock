// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Dock.Model.UnitTests
{
    public class TestDock : IDock
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public object Context { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IView Parent { get; set; }
        public IList<IView> Views { get; set; }
        public IView CurrentView { get; set; }
        public IView DefaultView { get; set; }
        public IView FocusedView { get; set; }
        public bool IsActive { get; set; }
        public bool CanGoBack => false;
        public bool CanGoForward => false;
        public IList<IDockWindow> Windows { get; set; }
        public string Dock { get; set; }
        public IDockFactory Factory { get; set; }

        public void GoBack()
        {
            throw new System.NotImplementedException();
        }

        public void GoForward()
        {
            throw new System.NotImplementedException();
        }

        public void Navigate(object root)
        {
            throw new System.NotImplementedException();
        }

        public void ShowWindows()
        {
            throw new System.NotImplementedException();
        }

        public void HideWindows()
        {
            throw new System.NotImplementedException();
        }

        public void ExitWindows()
        {
            throw new System.NotImplementedException();
        }
    }
}
