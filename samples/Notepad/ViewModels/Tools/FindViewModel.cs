using System.Runtime.Serialization;
using Dock.Model;
using Dock.Model.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Tools
{
    public class FindViewModel : Tool
    {
        private string _find;

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Find
        {
            get => _find;
            set => this.RaiseAndSetIfChanged(ref _find, value);
        }

        public void FindNext()
        {
            if (this.Context is IRootDock root)
            {
                if (root.Factory.FindDockable(root, (d) => d.Id == MainWindowViewModel.FilesId) is IDock files)
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
