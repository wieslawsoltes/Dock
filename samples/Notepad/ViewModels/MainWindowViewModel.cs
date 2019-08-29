using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model;
using Dock.Serializer;
using Notepad.ViewModels.Documents;
using ReactiveUI;

namespace Notepad.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
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

        private FileViewModel OpenFileViewModel(string path)
        {
            return new FileViewModel()
            {
                Path = path,
                Title = Path.GetFileName(path),
                Text = File.ReadAllText(path)
            };
        }

        private void SaveFileViewModel(FileViewModel fileViewModel)
        {
            File.WriteAllText(fileViewModel.Path, fileViewModel.Text);
        }

        private void UpdateFileViewModel(FileViewModel fileViewModel, string path)
        {
            fileViewModel.Path = path;
            fileViewModel.Title = Path.GetFileName(path);
        }

        private void AddFileViewModel(FileViewModel fileViewModel)
        {
            if (Factory.FindDockable(Layout, (d) => d.Id == "Files") is IDock files)
            {
                Factory.AddDockable(files, fileViewModel);
                Factory.SetActiveDockable(fileViewModel);
                Factory.SetFocusedDockable(Layout, fileViewModel);
            }
        }

        private FileViewModel GetFileViewModel()
        {
            if (Factory.FindDockable(Layout, (d) => d.Id == "Files") is IDock files)
            {
                return files.ActiveDockable as FileViewModel;
            }
            return null;
        }

        public void FileNew()
        {
            var untitledFileViewModel = new FileViewModel()
            {
                Path = string.Empty,
                Title = "Untitled",
                Text = ""
            };
            AddFileViewModel(untitledFileViewModel);
        }

        public async void FileOpen()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null)
            {
                var path = result.FirstOrDefault();
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

        private void CopyFileViewModels(IDock layout)
        {
            if (Factory.FindDockable(Layout, (d) => d.Id == "Files") is IDock sourceFiles)
            {
                if (Factory.FindDockable(layout, (d) => d.Id == "Files") is IDock targetFiles)
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
        }

        public void WindowSaveWindowLayout()
        {
            // TODO:
        }

        public void WindowApplyWindowLayout(IDock layout)
        {
            CopyFileViewModels(layout);
            Factory.InitLayout(layout);
            Layout.Close();
            Layout = layout;
        }

        public void WindowManageWindowLayouts()
        {
            // TODO:
        }

        public void WindowResetWindowLayout()
        {
            var layout = Factory.CreateLayout();
            CopyFileViewModels(layout);
            Factory.InitLayout(layout);
            Layout.Close();
            Layout = layout;
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
