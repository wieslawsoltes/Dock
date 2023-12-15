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
using Dock.Model.Mvvm.Controls;

namespace Notepad.ViewModels.Documents;

public class FileViewModel : Document
{
    private string _path = string.Empty;
    private string _text = string.Empty;
    private string _encoding = string.Empty;

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public string Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }
}
