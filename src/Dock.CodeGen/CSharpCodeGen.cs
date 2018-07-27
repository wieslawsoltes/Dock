using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.CodeGen
{
    public class CSharpCodeGen : ICodeGen
    {
        private StringBuilder _sb;
        private IDictionary<IView, string> _idViews;
        private IDictionary<IDockWindow, string> _idWindows;
        private int _viewCount;
        private int _windowCount;

        private void Write(string path, string text)
        {
            using (var fs = System.IO.File.Create(path))
            {
                using (var sw = new System.IO.StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(text);
                }
            }
        }

        private void Output(string text)
        {
            if (_sb != null)
            {
                _sb.AppendLine(text);
            }
        }

        private string FormatDouble(double value)
        {
            if (double.IsNaN(value))
            {
                return "double.NaN";
            }
            return $"{value.ToString(CultureInfo.GetCultureInfo("en-GB"))}";
        }

        private string FormatBool(bool value)
        {
            return value ? "true" : "false";
        }

        private void WriteWindow(IDockWindow window, string indent = "")
        {
            string id = $"window{_windowCount}";
            _windowCount++;

            if (!_idWindows.ContainsKey(window))
            {
                _idWindows[window] = id;
            }

            Output($"{indent}var {id} = new {window.GetType().Name}()");
            Output($"{indent}{{");
            Output($"{indent}    Id = \"{window.Id}\",");
            Output($"{indent}    X = {FormatDouble(window.X)},");
            Output($"{indent}    Y = {FormatDouble(window.Y)},");
            Output($"{indent}    Width = {FormatDouble(window.Width)},");
            Output($"{indent}    Height = {FormatDouble(window.Height)},");
            Output($"{indent}    Title = \"{window.Title}\",");

            Output($"{indent}}};");

            if (window.Layout is IDock dock)
            {
                WriteDock(dock, indent);
            }
            else
            {
                WriteDock(window.Layout, indent);
            }
        }

        private void WriteView(IView root, string indent = "")
        {
            string id = $"view{_viewCount}";
            _viewCount++;

            if (!_idViews.ContainsKey(root))
            {
                _idViews[root] = id;
            }

            Output($"{indent}var {id} = new {root.GetType().Name}()");
            Output($"{indent}{{");
            Output($"{indent}    Id = \"{root.Id}\",");
            Output($"{indent}    Title = \"{root.Title}\",");
            Output($"{indent}}};");
        }

        private void WriteDock(IDock root, string indent = "")
        {
            string id = $"view{_viewCount}";
            _viewCount++;

            if (!_idViews.ContainsKey(root))
            {
                _idViews[root] = id;
            }

            Output($"{indent}var {id} = new {root.GetType().Name}()");
            Output($"{indent}{{");
            Output($"{indent}    Id = \"{root.Id}\",");
            Output($"{indent}    Title = \"{root.Title}\",");
            Output($"{indent}    Proportion = {FormatDouble(root.Proportion)},");
            Output($"{indent}    IsActive = {FormatBool(root.IsActive)},");
            Output($"{indent}    IsCollapsable = {FormatBool(root.IsCollapsable)},");

            if (root is ILayoutDock layoutDock)
            {
                Output($"{indent}    Orientation = Orientation.{layoutDock.Orientation},");
            }

            //if (root is IRootDock rootDock)
            //{
            //    WriteWindow(rootDock.Window, indent);
            //}

            Output($"{indent}}};");

            if (root.Views != null)
            {
                foreach (var view in root.Views)
                {
                    if (view is IDock dock)
                    {
                        WriteDock(dock, indent);
                    }
                    else
                    {
                        WriteView(view, indent);
                    }
                }
            }

            if (root.Windows != null)
            {
                foreach (var window in root.Windows)
                {
                    WriteWindow(window, indent);
                }
            }
        }

        private void WriteViews(string indent, string valueId, IDock dock)
        {
            if (dock.Views != null && dock.Views.Count > 0)
            {
                if (dock.CurrentView != null)
                {
                    Output($"{indent}{valueId}.CurrentView = {_idViews[dock.CurrentView]};");
                }

                if (dock.DefaultView != null)
                {
                    Output($"{indent}{valueId}.DefaultView = {_idViews[dock.DefaultView]};");
                }

                Output($"{indent}{valueId}.Views = CreateList<IView>();");
                foreach (var view in dock.Views)
                {
                    Output($"{indent}{valueId}.Views.Add({_idViews[view]});");
                }
            }
        }

        private void WriteWindows(string indent, string valueId, IDock dock)
        {
            if (dock.Windows != null && dock.Windows.Count > 0)
            {
                Output($"{indent}{valueId}.Windows = CreateList<IDockWindow>();");
                foreach (var window in dock.Windows)
                {
                    Output($"{indent}{valueId}.Windows.Add({_idWindows[window]});");
                }

                foreach (var window in dock.Windows)
                {
                    if (window.Layout != null)
                    {
                        Output($"{indent}{_idWindows[window]}.Layout = {_idViews[window.Layout]};");
                    }
                }
            }
        }

        private void WriteLists(IView root, string indent = "")
        {
            foreach (var kvp in _idViews)
            {
                if (kvp.Key is IDock dock)
                {
                    WriteViews(indent, kvp.Value, dock);
                    WriteWindows(indent, kvp.Value, dock);
                }
            }
            Output($"{indent}return {_idViews[root]};");
        }

        private void WriterHeader()
        {
            Output(@"using System;
using System.Collections.Generic;
using AvaloniaDemo.INPC.Model;
using AvaloniaDemo.INPC.ViewModels.Documents;
using AvaloniaDemo.INPC.ViewModels.Tools;
using AvaloniaDemo.INPC.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Editor;
using Dock.Model;
using Dock.Model.Controls; 

namespace AvaloniaDemo.INPC.Factories
{
    public class DemoDockFactory : DockFactory
    {
        private object _context;

        public DemoDockFactory(object context)
        {
            _context = context;
        }

        public override IDock CreateLayout()
        {");
        }

        private void WriteFooter()
        {
            Output(@"        }

        public override void InitLayout(IView layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => _context,
                [nameof(ILayoutDock)] = () => _context,
                [nameof(IDocumentDock)] = () => _context,
                [nameof(IToolDock)] = () => _context,
                [nameof(ISplitterDock)] = () => _context,
                [nameof(IDockWindow)] = () => _context,
                [nameof(IDocumentTab)] = () => _context,
                [nameof(IToolTab)] = () => _context,
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }
}");
        }

        public void Generate(IView view, string path)
        {
            _sb = new StringBuilder();
            _idViews = new Dictionary<IView, string>();
            _idWindows = new Dictionary<IDockWindow, string>();
            _viewCount = 0;
            _windowCount = 0;

            string indent = "            ";

            WriterHeader();

            if (view is IDock dock)
            {
                WriteDock(dock, indent);
            }
            else
            {
                WriteView(view, indent);
            }

            WriteLists(view, indent);

            WriteFooter();

            var text = _sb.ToString();

            Write(path, text);

            _sb.Clear();
        }
    }
}
