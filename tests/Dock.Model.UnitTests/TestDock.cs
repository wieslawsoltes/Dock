// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Dock.Model.UnitTests
{
    public class TestDock : IDock
    {
        public string Id { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Title { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public object Context { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double Width { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double Height { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IView Parent { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IList<IView> Views { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IView CurrentView { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IView DefaultView { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IView FocusedView { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool CanGoBack => throw new System.NotImplementedException();
        public bool CanGoForward => throw new System.NotImplementedException();
        public IList<IDockWindow> Windows { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Dock { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDockFactory Factory { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

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
    }
}
