/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Dock.Model.Core;

namespace Dock.Model.Adapters;

/// <summary>
/// Host adapter for the <see cref="IDockWindow"/>.
/// </summary>
public class HostAdapter : IHostAdapter
{
    private readonly IDockWindow _window;

    /// <summary>
    /// Initializes new instance of the <see cref="HostAdapter"/> class.
    /// </summary>
    /// <param name="window">The window instance.</param>
    public HostAdapter(IDockWindow window)
    {
        _window = window;
    }

    /// <inheritdoc/>
    public void Save()
    {
        if (_window.Host is not null)
        {
            _window.Host.GetPosition(out var x, out var y);
            _window.X = x;
            _window.Y = y;

            _window.Host.GetSize(out var width, out var height);
            _window.Width = width;
            _window.Height = height;
        }
    }

    /// <inheritdoc/>
    public void Present(bool isDialog)
    {
        if (_window.Layout is null)
        {
            return;
        }

        if (_window.Host is null)
        {
            _window.Host = _window.Factory?.GetHostWindow(_window.Id);
            if (_window.Host is not null)
            {
                _window.Host.Window = _window;
            }
        }

        if (_window.Host is not null)
        {
            _window.Host.Present(isDialog);
            _window.Host.SetPosition(_window.X, _window.Y);
            _window.Host.SetSize(_window.Width, _window.Height);
            _window.Host.SetTopmost(_window.Topmost);
            _window.Host.SetTitle(_window.Title);
            _window.Host.SetLayout(_window.Layout);
            _window.Host.IsTracked = true;
        }
    }

    /// <inheritdoc/>
    public void Exit()
    {
        if (_window.Host is not null)
        {
            Save();
            _window.Host.IsTracked = false;
            _window.Host.Exit();
            _window.Host = null;
        }
    }
}
