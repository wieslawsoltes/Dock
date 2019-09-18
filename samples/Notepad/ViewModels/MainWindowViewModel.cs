using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model;
using Dock.Serializer;
using Notepad.ViewModels.Documents;
using Notepad.Views.Layouts;
using ReactiveUI;

namespace Notepad.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public const string DocumentsDockId = "Files";

        private IDockSerializer _serializer;
        private IFactory _factory;
        private IDock _layout;

        public IDockSerializer Serializer
        {
            get => _serializer;
            set => this.RaiseAndSetIfChanged(ref _serializer, value);
        }

        public IFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IDock Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        private Encoding GetEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.Default, true))
            {
                if (reader.Peek() >= 0)
                {
                    reader.Read();
                }
                return reader.CurrentEncoding;
            }
        }

        private FileViewModel OpenFileViewModel(string path)
        {
            Encoding encoding = GetEncoding(path);
            string text = File.ReadAllText(path, encoding);
            string title = Path.GetFileName(path);
            return new FileViewModel()
            {
                Path = path,
                Title = title,
                Text = text,
                Encoding = encoding.WebName
            };
        }

        private void SaveFileViewModel(FileViewModel fileViewModel)
        {
            File.WriteAllText(fileViewModel.Path, fileViewModel.Text, Encoding.GetEncoding(fileViewModel.Encoding));
        }

        private void UpdateFileViewModel(FileViewModel fileViewModel, string path)
        {
            fileViewModel.Path = path;
            fileViewModel.Title = Path.GetFileName(path);
        }

        private void AddFileViewModel(FileViewModel fileViewModel)
        {
            if (Layout.ActiveDockable is IDock active)
            {
                if (active.Factory.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock files)
                {
                    Factory.AddDockable(files, fileViewModel);
                    Factory.SetActiveDockable(fileViewModel);
                    Factory.SetFocusedDockable(Layout, fileViewModel);
                }
            }
        }

        private FileViewModel GetFileViewModel()
        {
            if (Layout.ActiveDockable is IDock active)
            {
                if (active.Factory.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock files)
                {
                    return files.ActiveDockable as FileViewModel;
                }
            }
            return null;
        }

        private FileViewModel GetUntitledFileViewModel()
        {
            return new FileViewModel()
            {
                Path = string.Empty,
                Title = "Untitled",
                Text = "",
                Encoding = Encoding.Default.WebName
            };
        }

        public void FileNew()
        {
            var untitledFileViewModel = GetUntitledFileViewModel();
            if (untitledFileViewModel != null)
            {
                AddFileViewModel(untitledFileViewModel);
            }
        }

        public async void FileOpen()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.AllowMultiple = true;
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null && result.Length > 0)
            {
                foreach (var path in result)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var untitledFileViewModel = OpenFileViewModel(path);
                        if (untitledFileViewModel != null)
                        {
                            AddFileViewModel(untitledFileViewModel);
                        }
                    }
                }
            }
        }

        public async void FileSave()
        {
            if (GetFileViewModel() is FileViewModel fileViewModel)
            {
                if (string.IsNullOrEmpty(fileViewModel.Path))
                {
                    await FileSaveAsImpl(fileViewModel);
                }
                else
                {
                    SaveFileViewModel(fileViewModel);
                }
            }
        }

        public async void FileSaveAs()
        {
            if (GetFileViewModel() is FileViewModel fileViewModel)
            {
                await FileSaveAsImpl(fileViewModel);
            }
        }

        public async Task FileSaveAsImpl(FileViewModel fileViewModel)
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.InitialFileName = fileViewModel.Title;
            dlg.DefaultExtension = "txt";
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    UpdateFileViewModel(fileViewModel, result);
                    SaveFileViewModel(fileViewModel);
                }
            }
        }

        public void FileExit()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }

        private void CopyDocuments(IDock source, IDock target, string id)
        {
            if (source.Factory.FindDockable(source, (d) => d.Id == id) is IDock sourceFiles
                && target.Factory.FindDockable(target, (d) => d.Id == id) is IDock targetFiles)
            {
                targetFiles.VisibleDockables.Clear();
                targetFiles.ActiveDockable = null;

                foreach (var visible in sourceFiles.VisibleDockables)
                {
                    targetFiles.VisibleDockables.Add(visible);
                }

                targetFiles.ActiveDockable = sourceFiles.ActiveDockable;
            }
        }

        public async void WindowSaveWindowLayout()
        {
            if (GetWindow() is Window onwer)
            {
                var window = new SaveWindowLayoutWindow();

                // TODO:

                await window.ShowDialog(onwer);
            }

            // TODO:

            if (Layout.ActiveDockable is IDock active)
            {
                var clone = (IDock)active.Clone();
                clone.Title = clone.Title + "-copy";
                active.Close();
                Layout.Factory.AddDockable(Layout, clone);
                Layout.Navigate(clone);
                Layout.Factory.SetFocusedDockable(Layout, clone);
                Layout.DefaultDockable = clone;
            }
        }

        public void WindowApplyWindowLayout(IDock dock)
        {
            if (Layout.ActiveDockable is IDock active && dock != active)
            {
                active.Close();
                CopyDocuments(active, dock, DocumentsDockId);
                Layout.Navigate(dock);
                Layout.Factory.SetFocusedDockable(Layout, dock);
                Layout.DefaultDockable = dock;
            }
        }

        public async void WindowManageWindowLayouts()
        {
            if (GetWindow() is Window onwer)
            {
                var window = new ManageWindowLayoutsWindow();

                // TODO:

                await window.ShowDialog(onwer);
            }
        }

        public void WindowResetWindowLayout()
        {
            if (Layout.ActiveDockable is IDock active)
            {
                var layout = Factory.CreateLayout();
                Factory.InitLayout(layout);
                CopyDocuments(active, layout, DocumentsDockId);
                Layout.Close();
                Layout = layout;
            }
        }

        private Window GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }
    }
}
