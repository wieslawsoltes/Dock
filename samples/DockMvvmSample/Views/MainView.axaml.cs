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
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;
using Dock.Settings;

namespace DockMvvmSample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        InitializeThemes();
        InitializeMenu();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeThemes()
    {
        var dark = false;
        var theme = this.Find<Button>("ThemeButton");
        if (theme is { })
        {
            theme.Click += (_, _) =>
            {
                dark = !dark;
                App.ThemeManager?.Switch(dark ? 1 : 0);
            };
        }
    }

    private void InitializeMenu()
    {
        var optionsIsDragEnabled = this.FindControl<MenuItem>("OptionsIsDragEnabled");
        if (optionsIsDragEnabled is { })
        {
            optionsIsDragEnabled.Click += (_, _) =>
            {
                if (VisualRoot is Window window)
                {
                    var isEnabled = window.GetValue(DockProperties.IsDragEnabledProperty);
                    window.SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
                }
            };
        }

        var optionsIsDropEnabled = this.FindControl<MenuItem>("OptionsIsDropEnabled");
        if (optionsIsDropEnabled is { })
        {
            optionsIsDropEnabled.Click += (_, _) =>
            {
                if (VisualRoot is Window window)
                {
                    var isEnabled = window.GetValue(DockProperties.IsDropEnabledProperty);
                    window.SetValue(DockProperties.IsDropEnabledProperty, !isEnabled);
                }
            };
        }
    }
}
