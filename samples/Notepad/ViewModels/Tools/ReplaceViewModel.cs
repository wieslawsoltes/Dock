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
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Notepad.ViewModels.Documents;

namespace Notepad.ViewModels.Tools;

public class ReplaceViewModel : Tool
{
    private string _find = string.Empty;
    private string _replace = string.Empty;

    public string Find
    {
        get => _find;
        set => SetProperty(ref _find, value);
    }

    public string Replace
    {
        get => _replace;
        set => SetProperty(ref _replace, value);
    }

    public void ReplaceNext()
    {
        if (Context is IRootDock root && root.ActiveDockable is IDock active)
        {
            if (active.Factory?.FindDockable(active, (d) => d.Id == "Files") is IDock files)
            {
                if (files.ActiveDockable is FileViewModel fileViewModel)
                {
                    // TODO: 
                }
            }
        }
    }
}
