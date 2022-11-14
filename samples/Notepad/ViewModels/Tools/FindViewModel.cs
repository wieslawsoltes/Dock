using System;
using System.Linq;
using System.Text;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Tools;

public class FindViewModel : Tool
{
    private string _find = string.Empty;

    public string Find
    {
        get => _find;
        set => this.RaiseAndSetIfChanged(ref _find, value);
    }

    public void FindNext()
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
                        start++;
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