using Dock.Model.Controls;
using System;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Notepad.ViewModels.Documents;

namespace Notepad.ViewModels.Tools;

public class FindViewModel : Tool
{
    private string _find = string.Empty;

    public string Find
    {
        get => _find;
        set => SetProperty(ref _find, value);
    }

    public void FindNext()
    {
        if (string.IsNullOrEmpty(Find))
        {
            return;
        }

        if (Context is IRootDock root && root.ActiveDockable is IDock active)
        {
            if (active.Factory?.FindDockable(active, d => d.Id == "Files") is IDock files)
            {
                if (files.ActiveDockable is FileViewModel fileViewModel)
                {
                    var start = fileViewModel.SelectionEnd;
                    if (start < 0 || start > fileViewModel.Text.Length)
                    {
                        start = 0;
                    }

                    var index = fileViewModel.Text.IndexOf(Find, start, StringComparison.CurrentCultureIgnoreCase);
                    if (index >= 0)
                    {
                        fileViewModel.SelectionStart = index;
                        fileViewModel.SelectionEnd = index + Find.Length;
                        fileViewModel.CaretIndex = fileViewModel.SelectionEnd;
                    }
                }
            }
        }
    }
}
