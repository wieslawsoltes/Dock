// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Dock.Model
{
    public class HostWindow : IHostWindow
    {
        public bool IsTracked { get; set; }

        public IDockWindow Window { get; set; }

        public void Present(bool isDialog)
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void SetPosition(double x, double y)
        {
            throw new NotImplementedException();
        }

        public void GetPosition(out double x, out double y)
        {
            throw new NotImplementedException();
        }

        public void SetSize(double width, double height)
        {
            throw new NotImplementedException();
        }

        public void GetSize(out double width, out double height)
        {
            throw new NotImplementedException();
        }

        public void SetTopmost(bool topmost)
        {
            throw new NotImplementedException();
        }

        public void SetTitle(string title)
        {
            throw new NotImplementedException();
        }

        public void SetLayout(IDock layout)
        {
            throw new NotImplementedException();
        }
    }
}
