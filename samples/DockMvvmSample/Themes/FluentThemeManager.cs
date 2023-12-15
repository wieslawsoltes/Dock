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
using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace DockMvvmSample.Themes;

public class FluentThemeManager : IThemeManager
{
    private static readonly Uri BaseUri = new("avares://DockMvvmSample/Styles");

    private static readonly FluentTheme Fluent = new()
    {
    };

    private static readonly Styles DockFluent = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockFluentTheme.axaml")
        }
    };

    private static readonly Styles FluentDark = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://DockMvvmSample/Themes/FluentDark.axaml")
        }
    };

    private static readonly Styles FluentLight = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://DockMvvmSample/Themes/FluentLight.axaml")
        }
    };

    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        switch (index)
        {
            // Fluent Light
            case 0:
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                Application.Current.Styles[0] = Fluent;
                Application.Current.Styles[1] = DockFluent;
                Application.Current.Styles[2] = FluentLight;
                break;
            }
            // Fluent Dark
            case 1:
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                Application.Current.Styles[0] = Fluent;
                Application.Current.Styles[1] = DockFluent;
                Application.Current.Styles[2] = FluentDark;
                break;
            }
        }
    }

    public void Initialize(Application application)
    {
        application.Styles.Insert(0, Fluent);
        application.Styles.Insert(1, DockFluent);
        application.Styles.Insert(2, FluentLight);
    }
}
