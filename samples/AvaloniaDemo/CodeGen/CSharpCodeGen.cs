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

        private void WriteWindow(IDockWindow window, string indent = "")
        {
            _windowCount++;

            string id = $"window{_windowCount}";
            if (!_idWindows.ContainsKey(window))
            {
                _idWindows[window] = id;
            }

            Output($"{indent}var {id} = new {window.GetType().Name}()");
            Output($"{indent}{{");
            Output($"{indent}    Id = \"{window.Id}\",");
            Output($"{indent}    X = \"{window.X}\",");
            Output($"{indent}    Y = \"{window.Y}\",");
            Output($"{indent}    Width = \"{window.Width}\",");
            Output($"{indent}    Height = \"{window.Height}\",");
            Output($"{indent}    Title = \"{window.Title}\",");

            Output($"{indent}}};");
            Output($"{indent}");

            WriteView(window.Layout, indent);
        }

        private void WriteView(IView view, string indent = "")
        {
            _viewCount++;

            string id = $"view{_viewCount}";
            if (!_idViews.ContainsKey(view))
            {
                _idViews[view] = id;
            }

            Output($"{indent}var {id} = new {view.GetType().Name}()");
            Output($"{indent}{{");

            Output($"{indent}    Id = \"{view.Id}\",");

            if (view is IDock viewDock)
            {
                Output($"{indent}    Dock = \"{viewDock.Dock}\",");
            }

            Output($"{indent}    Width = \"{view.Width}\",");
            Output($"{indent}    Height = \"{view.Height}\",");
            Output($"{indent}    Title = \"{view.Title}\",");

            Output($"{indent}}};");
            Output($"{indent}");

            if (view is IViewsHost viewViewsHost)
            {
                if (viewViewsHost.Views != null && viewViewsHost.Views.Count > 0)
                {
                    for (int i = 0; i < viewViewsHost.Views.Count; i++)
                    {
                        var child = viewViewsHost.Views[i];
                        WriteView(child, indent);
                    }
                }
            }

            if (view is IWindowsHost viewWindowsHost)
            {
                if (viewWindowsHost.Windows != null && viewWindowsHost.Windows.Count > 0)
                {
                    for (int i = 0; i < viewWindowsHost.Windows.Count; i++)
                    {
                        var child = viewWindowsHost.Windows[i];
                        WriteWindow(child, indent);
                    }
                }
            }
        }

        private void WriteReferences(string indent = "")
        {
            foreach (var kvp in _idViews)
            {
                IView view = kvp.Key;
                string id = kvp.Value;

                if (view is IViewsHost viewViewsHost)
                {
                    if (viewViewsHost.Views != null && viewViewsHost.Views.Count > 0)
                    {
                        if (viewViewsHost.CurrentView != null)
                        {
                            Output($"{indent}{id}.CurrentView = {_idViews[viewViewsHost.CurrentView]};");
                        }

                        if (viewViewsHost.DefaultView != null)
                        {
                            Output($"{indent}{id}.CurrentView = {_idViews[viewViewsHost.DefaultView]};");
                        }

                        Output($"{indent}{id}.Views = new ObservableCollection<IView>");
                        Output($"{indent}{{");

                        for (int i = 0; i < viewViewsHost.Views.Count; i++)
                        {
                            var child = viewViewsHost.Views[i];
                            Output($"{indent}    {_idViews[child]},");
                        }

                        Output($"{indent}}};");
                        Output($"");
                    }
                }

                if (view is IWindowsHost viewWindowsHost)
                {
                    if (viewWindowsHost.Windows != null && viewWindowsHost.Windows.Count > 0)
                    {
                        Output($"{indent}{id}.Windows = new ObservableCollection<IDockWindow>");
                        Output($"{indent}{{");

                        for (int i = 0; i < viewWindowsHost.Windows.Count; i++)
                        {
                            var child = viewWindowsHost.Windows[i];
                            Output($"{indent}    {_idWindows[child]},");
                        }

                        Output($"{indent}}};");
                        Output($"");
                    }
                }
            };
        }

        private void WriterHeader()
        {
            Output(@"using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model; 
using Dock.Model.Controls; 

namespace ViewModels
{
    /// <inheritdoc/>
    public class EmptyDockFactory : DockFactory
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
                [nameof(IDockWindow)] = () => context
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            this.Update(layout, context, null);

            if (layout is IWindowsHost layoutWindowsHost)
            {
                layoutWindowsHost.ShowWindows();
                if (layout is IViewsHost layoutViewsHost)
                {
                    layoutViewsHost.CurrentView = layoutViewsHost.DefaultView;
                    if (layoutViewsHost.CurrentView is IWindowsHost currentViewWindowsHost)
                    {
                        currentViewWindowsHost.ShowWindows();
                    }
                }
            }
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

            WriteView(view, indent);
            WriteReferences(indent);

            WriteFooter();

            var text = _sb.ToString();

            Write(path, text);

            _sb.Clear();
        }
    }
}
