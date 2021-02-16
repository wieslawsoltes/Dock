using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace Notepad.ViewModels.Documents
{
    public class FileViewModel : Document
    {
        private string _path = string.Empty;
        private string _text = string.Empty;
        private string _encoding = string.Empty;

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public string Encoding
        {
            get => _encoding;
            set => this.RaiseAndSetIfChanged(ref _encoding, value);
        }
    }
}
