using System.Runtime.Serialization;
using Dock.Model.Controls;
using ReactiveUI;

namespace Notepad.ViewModels.Documents
{
    public class FileViewModel : Document
    {
        private string _path;
        private string _text;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }
    }
}
