// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace Dock.Avalonia.Internal;

internal static class ScrollViewerMouseWheelHookHelper
{
    public static IDisposable? Attach(ScrollViewer? scrollViewer, Orientation orientation)
    {
        if (scrollViewer is null)
        {
            return null;
        }

        void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (TabStripMouseWheelScrollHelper.TryHandle(scrollViewer, orientation, e.Delta))
            {
                e.Handled = true;
            }
        }

        scrollViewer.PointerWheelChanged += OnPointerWheelChanged;
        return new DelegateDisposable(() => scrollViewer.PointerWheelChanged -= OnPointerWheelChanged);
    }

    private sealed class DelegateDisposable : IDisposable
    {
        private Action? _dispose;

        public DelegateDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
