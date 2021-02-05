using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Tools
{
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
}
