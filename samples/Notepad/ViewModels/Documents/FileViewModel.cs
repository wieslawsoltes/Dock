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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Encoding
        {
            get => _encoding;
            set => this.RaiseAndSetIfChanged(ref _encoding, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SelectionStart
        {
            get => _selectionStart;
            set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SelectionEnd
        {
            get => _selectionEnd;
            set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int CaretIndex
        {
            get => _caretIndex;
            set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
        }
    }
}
