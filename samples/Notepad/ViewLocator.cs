using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Core;
using Notepad.ViewModels.Documents;
using Notepad.ViewModels.Tools;
using Notepad.Views.Documents;
using Notepad.Views.Tools;

namespace Notepad;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        switch (data)
        {
            case FileViewModel:
                return new FileView();
            case FindViewModel:
                return new FindView();
            case ReplaceViewModel:
                return new ReplaceView();
            default:
                return new TextBlock { Text = "Create Instance Failed: " };
        }
    }

    public bool Match(object? data)
    {
        return data is ObservableObject || data is IDockable;
    }
}
