
using System;
using Dock.Model.Controls;

namespace Dock.Model
{
    public class DockWindow : IDockWindow
    {
        private IHostAdapter _hostAdapter;

        public string Id { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool Topmost { get; set; }

        public string Title { get; set; }

        public IDockable? Owner { get; set; }

        public IFactory? Factory { get; set; }

        public IRootDock? Layout { get; set; }

        public IHostWindow? Host { get; set; }

        public DockWindow()
        {
            Id = nameof(IDockWindow);
            Title = nameof(IDockWindow);
            _hostAdapter = new HostAdapter(this);
        }

        public void Save()
        {
            _hostAdapter.Save();
        }

        public void Present(bool isDialog)
        {
            _hostAdapter.Present(isDialog);
        }

        public void Exit()
        {
            _hostAdapter.Exit();
        }

        public IDockWindow? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
