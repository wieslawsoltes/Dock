using System;
using System.Text;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Tools;

public class ReplaceViewModel : Tool
{
    private string _find = string.Empty;
    private string _replace = string.Empty;

    public string Find
    {
        get => _find;
        set => this.RaiseAndSetIfChanged(ref _find, value);
    }

    public string Replace
    {
        get => _replace;
        set => this.RaiseAndSetIfChanged(ref _replace, value);
    }

    public void ReplaceNext()
    {
        if (Context is IRootDock root 
            && root.ActiveDockable is IDock active
            && _find.Length > 0)
        {
            if (active.Factory?.FindDockable(active, (d) => d.Id == "Files") is IDock files)
            {
                if (files.ActiveDockable is FileViewModel fileViewModel)
                {
                    var start = Math.Max(fileViewModel.SelectionStart, 0);
                    if (fileViewModel.SelectedText == _find)
                    {
                        fileViewModel.SelectedText = _replace;
                        start++;
                    }
                    var ix = fileViewModel.Text.IndexOf(_find, start);
                    if (ix >= 0)
                    {
                        fileViewModel.SelectionStart = ix;
                        fileViewModel.SelectionEnd = ix + _find.Length;
                    }
                }
            }
        }
    }
}