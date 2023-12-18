using Dock.Model.Controls;
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
