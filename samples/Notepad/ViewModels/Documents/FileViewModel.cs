using System.Runtime.Serialization;
using System.Text;
using Dock.Model.Controls;
using ReactiveUI;

namespace Notepad.ViewModels.Documents
{
    public class FileViewModel : Document
    {
        private string _path;
        private string _text;
        private string _encoding;
        private int _selectionStart;
        private int _selectionEnd;
        private int _caretIndex;

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
