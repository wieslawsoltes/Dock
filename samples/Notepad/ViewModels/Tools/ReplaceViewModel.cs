using System.Runtime.Serialization;
using Dock.Model;
using Dock.Model.Controls;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels.Tools
{
    public class ReplaceViewModel : Tool
    {
        private string _find;
        private string _replace;

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Find
        {
            get => _find;
            set => this.RaiseAndSetIfChanged(ref _find, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Replace
        {
            get => _replace;
            set => this.RaiseAndSetIfChanged(ref _replace, value);
        }

        public void ReplaceNext()
        {
            if (this.Context is IRootDock root)
            {
                if (root.Factory.FindDockable(root, (d) => d.Id == "Files") is IDock files)
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
