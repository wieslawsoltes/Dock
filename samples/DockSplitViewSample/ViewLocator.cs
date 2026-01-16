// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DockSplitViewSample.ViewModels;
using DockSplitViewSample.Views;

namespace DockSplitViewSample;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        return data switch
        {
            NavigationViewModel => new NavigationView(),
            HomeViewModel => new HomeView(),
            SettingsViewModel => new SettingsView(),
            AboutViewModel => new AboutView(),
            _ => new TextBlock { Text = "View not found: " + data.GetType().Name }
        };
    }

    public bool Match(object? data)
    {
        return data is NavigationViewModel or HomeViewModel or SettingsViewModel or AboutViewModel;
    }
}
