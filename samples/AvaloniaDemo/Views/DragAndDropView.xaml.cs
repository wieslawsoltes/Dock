using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Model;

namespace AvaloniaDemo.Views
{
    public class DragAndDrop : UserControl
    {
        private TextBlock? _DropState;
        private TextBlock? _DragState;
        private Border? _DragMe;
        private int DragCount = 0;

        public DragAndDrop()
        {
            this.InitializeComponent();
            if (_DragMe != null)
            {
                _DragMe.PointerPressed += DoDrag;
            }
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private async void DoDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            DataObject dragData = new DataObject();
            dragData.Set(DataFormats.Text, $"You have dragged text {++DragCount} times");
            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
            switch (result)
            {
                case DragDropEffects.Copy:
                    if (_DragState != null)
                    {
                        _DragState.Text = "The text was copied";
                    }
                    break;
                case DragDropEffects.Link:
                    if (_DragState != null)
                    {
                        _DragState.Text = "The text was linked";
                    }
                    break;
                case DragDropEffects.None:
                    if (_DragState != null)
                    {
                        _DragState.Text = "The drag operation was canceled";
                    }
                    break;
            }
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);
            if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Text))
            {
                if (_DropState != null)
                {
                    _DropState.Text = e.Data.GetText();
                }
            }
            else if (e.Data.Contains(DataFormats.FileNames))
            {
                if (_DropState != null)
                {
                    var values = e.Data.GetFileNames();
                    if (values is {})
                    {
                        _DropState.Text = string.Join(Environment.NewLine, values);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _DropState = this.Find<TextBlock>("DropState");
            _DragState = this.Find<TextBlock>("DragState");
            _DragMe = this.Find<Border>("DragMe");
        }
    }
}
