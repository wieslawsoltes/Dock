using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dock.Model;

namespace AvaloniaDemo
{
    public class CodeGen
    {
        private StringBuilder _sb;

        private IDictionary<IView, string> _idViews;
        private IDictionary<IDockWindow, string> _idWindows;

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
            _sb.AppendLine(text);
        }

        private void GenerateWindows(IDockWindow window, ref int count, string indent = "")
        {
            count++;

            string id = $"window{count}";
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

            GenerateObjects(window.Layout, ref count, indent);
        }

        private void GenerateObjects(IView view, ref int count, string indent = "")
        {
            count++;

            string id = $"view{count}";
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
                        GenerateObjects(child, ref count, indent);
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
                        GenerateWindows(child, ref count, indent);
                    }
                }
            }
        }

        private void GenerateReferences(string indent = "")
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
                    }
                }

                Output($"{indent}");
            };
        }

        public void Generate(IView root, string path)
        {
            _sb = new StringBuilder();
            _idViews = new Dictionary<IView, string>();
            _idWindows = new Dictionary<IDockWindow, string>();

            Output($"using System;");
            Output($"using System.Collections.Generic;");
            Output($"using System.Collections.ObjectModel;");
            Output($"using Dock.Model; ");
            Output($"using Dock.Model.Controls; ");
            Output($"");
            Output($"namespace ViewModels");
            Output($"{{");

            Output($"    /// <inheritdoc/>");
            Output($"    public class EmptyDockFactory : DockFactory");
            Output($"    {{");
            Output($"        /// <inheritdoc/>");
            Output($"        public override IDock CreateLayout()");
            Output($"        {{");

            int count = 0;

            GenerateObjects(root, ref count, "            ");
            GenerateReferences("            ");

            Output($"        }}");
            Output($"");
            Output($"        /// <inheritdoc/>");
            Output($"        public override void InitLayout(IView layout, object context)");
            Output($"        {{");
            Output($"            this.ContextLocator = new Dictionary<string, Func<object>>");
            Output($"            {{");
            Output($"                [nameof(IRootDock)] = () => context,");
            Output($"                [nameof(ILayoutDock)] = () => context,");
            Output($"                [nameof(IDocumentDock)] = () => context,");
            Output($"                [nameof(IToolDock)] = () => context,");
            Output($"                [nameof(ISplitterDock)] = () => context,");
            Output($"                [nameof(IDockWindow)] = () => context");
            Output($"            }};");
            Output($"");
            Output($"            this.HostLocator = new Dictionary<string, Func<IDockHost>>");
            Output($"            {{");
            Output($"                [nameof(IDockWindow)] = () => new HostWindow()");
            Output($"            }};");
            Output($"");
            Output($"            this.Update(layout, context, null);");
            Output($"");
            Output($"            if (layout is IWindowsHost layoutWindowsHost)");
            Output($"            {{");
            Output($"                layoutWindowsHost.ShowWindows();");
            Output($"                if (layout is IViewsHost layoutViewsHost)");
            Output($"                {{");
            Output($"                    layoutViewsHost.CurrentView = layoutViewsHost.DefaultView;");
            Output($"                    if (layoutViewsHost.CurrentView is IWindowsHost currentViewWindowsHost)");
            Output($"                    {{");
            Output($"                        currentViewWindowsHost.ShowWindows();");
            Output($"                    }}");
            Output($"                }}");
            Output($"            }}");
            Output($"        }}");
            Output($"    }}");

            Output($"}}");
            Output($"");

            var text = _sb.ToString();

            Write(path, text);

            _sb.Clear();
        }
    }
}
