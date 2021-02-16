using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Notepad.ViewModels;

namespace Notepad.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (DataContext is IDropTarget dropTarget)
            {
                dropTarget.DragOver(sender, e);
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (DataContext is IDropTarget dropTarget)
            {
                dropTarget.Drop(sender, e);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
