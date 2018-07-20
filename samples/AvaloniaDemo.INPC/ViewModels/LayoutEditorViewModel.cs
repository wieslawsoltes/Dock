using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo.INPC.ViewModels
{
    public class LayoutEditorViewModel : ReactiveObject, ILayoutEditorViewModel
    {
        private IDockFactory _factory;
        private IView _layout;

        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IView Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        private void InsertLayout(IDock dock, int index, object context)
        {
            var view = _factory.CreateLayoutDock();
            view.Id = nameof(ILayoutDock);
            view.Title = nameof(ILayoutDock);
            view.Proportion = double.NaN;
            view.Orientation = Orientation.Horizontal;
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertRoot(IDock dock, int index, object context)
        {
            var view = _factory.CreateRootDock();
            view.Id = nameof(IRootDock);
            view.Title = nameof(IRootDock);
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertSplitter(IDock dock, int index, object context)
        {
            var view = _factory.CreateSplitterDock();
            view.Id = nameof(ISplitterDock);
            view.Title = nameof(ISplitterDock);
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertDocument(IDock dock, int index, object context)
        {
            var view = _factory.CreateDocumentDock();
            view.Id = nameof(IDocumentDock);
            view.Title = nameof(IDocumentDock);
            view.Proportion = double.NaN;
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertTool(IDock dock, int index, object context)
        {
            var view = _factory.CreateToolDock();
            view.Id = nameof(IToolDock);
            view.Title = nameof(IToolDock);
            view.Proportion = double.NaN;
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertView(IDock dock, int index, object context)
        {
            var view = _factory.CreateView();
            view.Id = nameof(IView);
            view.Title = nameof(IView);
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertToolTab(IDock dock, int index, object context)
        {
            var view = _factory.CreateToolTab();
            view.Id = nameof(IToolTab);
            view.Title = nameof(IToolTab);
            _factory.InsertView(dock, view, index, context);
        }

        private void InsertDocumentTab(IDock dock, int index, object context)
        {
            var view = _factory.CreateDocumentTab();
            view.Id = nameof(IDocumentTab);
            view.Title = nameof(IDocumentTab);
            _factory.InsertView(dock, view, index, context);
        }

        public virtual void AddLayout(IDock dock)
        {
            InsertLayout(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddRoot(IDock dock)
        {
            InsertRoot(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddSplitter(IDock dock)
        {
            InsertSplitter(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddDocument(IDock dock)
        {
            InsertDocument(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddTool(IDock dock)
        {
            InsertTool(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddView(IDock dock)
        {
            InsertView(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddToolTab(IDock dock)
        {
            InsertToolTab(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void AddDocumentTab(IDock dock)
        {
            InsertDocumentTab(dock, dock.Views != null ? dock.Views.Count : 0, dock.Context);
        }

        public virtual void InsertLayoutBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertLayout(parent, index, parent.Context);
            }
        }

        public virtual void InsertRootBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertRoot(parent, index, parent.Context);
            }
        }

        public virtual void InsertSplitterBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertSplitter(parent, index, parent.Context);
            }
        }

        public virtual void InsertDocumentBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertDocument(parent, index, parent.Context);
            }
        }

        public virtual void InsertToolBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertTool(parent, index, parent.Context);
            }
        }

        public virtual void InsertViewBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertView(parent, index, parent.Context);
            }
        }

        public virtual void InsertToolTabBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertToolTab(parent, index, parent.Context);
            }
        }

        public virtual void InsertDocumentTabBefore(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock);
                InsertDocumentTab(parent, index, parent.Context);
            }
        }

        public virtual void InsertLayoutAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertLayout(parent, index, parent.Context);
            }
        }

        public virtual void InsertRootAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertRoot(parent, index, parent.Context);
            }
        }

        public virtual void InsertSplitterAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertSplitter(parent, index, parent.Context);
            }
        }

        public virtual void InsertDocumentAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertDocument(parent, index, parent.Context);
            }
        }

        public virtual void InsertToolAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertTool(parent, index, parent.Context);
            }
        }

        public virtual void InsertViewAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertView(parent, index, parent.Context);
            }
        }

        public virtual void InsertToolTabAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertToolTab(parent, index, parent.Context);
            }
        }

        public virtual void InsertDocumentTabAfter(IDock dock)
        {
            if (dock.Parent is IDock parent && parent.Views != null)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertDocumentTab(parent, index, parent.Context);
            }
        }

        private void Copy(IView source, IView destination, bool bCopyViews, bool bCopyWindows)
        {
            destination.Id = source.Id;
            destination.Title = source.Title;

            if (source is ILayoutDock sourceLayoutDock && destination is ILayoutDock destinationLayoutDock)
            {
                destinationLayoutDock.Proportion = sourceLayoutDock.Proportion;
                destinationLayoutDock.Orientation = sourceLayoutDock.Orientation;
            }

            if (source is ITabDock sourceTabDock && destination is ITabDock destinationTabDock)
            {
                destinationTabDock.Proportion = sourceTabDock.Proportion;
            }

            if (source is IDock sourceDock && destination is IDock destinationDock)
            {
                if (bCopyViews)
                {
                    destinationDock.Views = sourceDock.Views;
                    destinationDock.CurrentView = sourceDock.CurrentView;
                    destinationDock.DefaultView = sourceDock.DefaultView;
                    destinationDock.FocusedView = sourceDock.FocusedView;
                    destinationDock.IsActive = sourceDock.IsActive;
                }

                if (bCopyWindows)
                {
                    destinationDock.Windows = sourceDock.Windows;
                }
            }
        }

        public virtual void ConvertToLayout(IDock dock)
        {
            var layout = _factory.CreateLayoutDock();
            Copy(dock, layout, true, true);
            _factory.Update(layout, dock.Context, dock.Parent);
            _factory.Replace(dock, layout);
        }

        public virtual void ConvertToRoot(IDock dock)
        {
            var root = _factory.CreateRootDock();
            Copy(dock, root, true, true);
            _factory.Update(root, dock.Context, dock.Parent);
            _factory.Replace(dock, root);
        }

        public virtual void ConvertToSplitter(IDock dock)
        {
            var splitter = _factory.CreateSplitterDock();
            Copy(dock, splitter, false, false);
            _factory.Update(splitter, dock.Context, dock.Parent);
            _factory.Replace(dock, splitter);
        }

        public virtual void ConvertToDocument(IDock dock)
        {
            var document = _factory.CreateDocumentDock();
            Copy(dock, document, true, false);
            _factory.Update(document, dock.Context, dock.Parent);
            _factory.Replace(dock, document);
        }

        public virtual void ConvertToTool(IDock dock)
        {
            var tool = _factory.CreateToolDock();
            Copy(dock, tool, true, false);
            _factory.Update(tool, dock.Context, dock.Parent);
            _factory.Replace(dock, tool);
        }

        public virtual void ConvertToView(IDock dock)
        {
            var view = _factory.CreateView();
            Copy(dock, view, false, true);
            _factory.Update(view, dock.Context, dock.Parent);
            _factory.Replace(dock, view);
        }

        public virtual void ConvertToToolTab(IDock dock)
        {
            var toolTab = _factory.CreateToolTab();
            Copy(dock, toolTab, false, true);
            _factory.Update(toolTab, dock.Context, dock.Parent);
            _factory.Replace(dock, toolTab);
        }

        public virtual void ConvertToDocumentTab(IDock dock)
        {
            var documentTab = _factory.CreateDocumentTab();
            Copy(dock, documentTab, false, true);
            _factory.Update(documentTab, dock.Context, dock.Parent);
            _factory.Replace(dock, documentTab);
        }
    }
}
