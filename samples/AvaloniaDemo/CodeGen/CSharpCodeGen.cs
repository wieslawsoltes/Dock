// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Text;
using Dock.Model;

namespace AvaloniaDemo.CodeGen
{
    /// <summary>
    /// CSharp code generator.
    /// </summary>
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
            return $"{value}";
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
            Output($"{indent}    Width = {FormatDouble(root.Width)},");
            Output($"{indent}    Height = {FormatDouble(root.Height)},");
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
            Output($"{indent}    Width = {FormatDouble(root.Width)},");
            Output($"{indent}    Height = {FormatDouble(root.Height)},");
            Output($"{indent}    Title = \"{root.Title}\",");
            Output($"{indent}    Dock = \"{root.Dock}\",");
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
using AvaloniaDemo.ViewModels.Documents;
using AvaloniaDemo.ViewModels.Tools;
using AvaloniaDemo.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Model; 
using Dock.Model.Controls; 

namespace AvaloniaDemo.ViewModels
{
    /// <inheritdoc/>
    public class DemoDockFactory : DockFactory
    {
        /// <inheritdoc/>
        public override IDock CreateLayout()
        {");
        }

        private void WriteFooter()
        {
            Output(@"        }

        /// <inheritdoc/>
        public override void InitLayout(IView layout, object context)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => context,
                [nameof(ILayoutDock)] = () => context,
                [nameof(IDocumentDock)] = () => context,
                [nameof(IToolDock)] = () => context,
                [nameof(ISplitterDock)] = () => context,
                [nameof(IDockWindow)] = () => context,
                [nameof(IDocumentTab)] = () => context,
                [nameof(IToolTab)] = () => context
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout, context);
        }
    }
}");
        }

        /// <inheritdoc/>
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
