using System.Runtime.Serialization;
using Dock.Model.Controls;
using ReactiveUI;

namespace Notepad.ViewModels.Documents
{
    public class FileViewModel : Document
    {
        private string _path = string.Empty;
        private string _text = string.Empty;
        private string _encoding = string.Empty;

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Encoding
        {
            get => _encoding;
            set => this.RaiseAndSetIfChanged(ref _encoding, value);
        }
    }
}
