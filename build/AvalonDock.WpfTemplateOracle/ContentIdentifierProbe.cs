using System;
using System.Collections.Generic;
using System.IO;
#if WPF_REFERENCE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
#else
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;
#endif

namespace AvalonDock.ReferenceProbes;

// The same public operations run against the private WPF reference and the adapter.
internal static class ContentIdentifierProbe
{
    internal static IEnumerable<string> Operations() => new[]
    {
        "defaults", "named", "unnamed", "equal-content", "explicit-id", "empty-id", "space-id", "same-content",
        "same-content-null-id", "replace-named", "rename-control", "remove-content", "reset-id",
        "same-id", "clear-id", "changing-id", "changed-id", "changed-clear-id", "changing-content",
        "changed-content", "changing-throw", "changed-throw", "id-redirect", "id-throw",
        "binding-oneway", "binding-twoway", "binding-assigned", "binding-reset", "binding-explicit",
        "item-null", "item-empty", "item-named", "item-reset", "item-model-id"
    };

    internal static void Run(string output)
    {
        List<string> rows = new() { "scenario\tstage\tstate\terror\tevents" };
        foreach (bool tools in new[] { false, true })
        foreach (string operation in Operations()) rows.AddRange(Observe(tools, operation));
        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} content-identifier observations.");
    }

    internal static string[] Observe(bool tools, string operation)
    {
        using Workspace workspace = new(tools, operation.StartsWith("item-", StringComparison.Ordinal));
        LayoutContent content = workspace.Content;
        List<string> rows = new();
        string scenario = (tools ? "tool-" : "document-") + operation;
        Take("before", "none");
        string error = "none";
        try
        {
            switch (operation)
            {
                case "defaults": break;
                case "named": content.Content = workspace.First; break;
                case "unnamed": content.Content = new Border(); break;
                case "equal-content":
                    content.Content = workspace.EqualFirst;
                    Take("content", "none");
                    content.Content = workspace.EqualSecond;
                    break;
                case "explicit-id": case "empty-id": case "space-id":
                    content.ContentId = operation == "explicit-id" ? "explicit" : operation == "empty-id" ? "" : " ";
                    Take("identifier", "none");
                    content.Content = workspace.First;
                    break;
                case "same-content": case "same-content-null-id": case "replace-named":
                case "rename-control": case "remove-content": case "reset-id":
                    content.Content = workspace.First;
                    Take("content", "none");
                    if (operation is "same-content-null-id" or "reset-id")
                    {
                        content.ContentId = null;
                        Take("reset", "none");
                    }
                    if (operation == "rename-control") workspace.First.Name = "Renamed";
                    else content.Content = operation is "replace-named" or "reset-id" ? workspace.Second
                        : operation == "remove-content" ? null : workspace.First;
                    break;
                case "same-id": case "clear-id":
                    content.ContentId = "explicit";
                    Take("identifier", "none");
                    if (operation == "same-id") content.ContentId = "explicit";
                    else content.ClearValue(LayoutContent.ContentIdProperty);
                    break;
                case "changing-id": workspace.BeforeContent = () => content.ContentId = "callback"; content.Content = workspace.First; break;
                case "changed-id": workspace.AfterContent = () => content.ContentId = "callback"; content.Content = workspace.First; break;
                case "changed-clear-id":
                    content.ContentId = "explicit";
                    Take("identifier", "none");
                    workspace.AfterContent = () => content.ContentId = null;
                    content.Content = workspace.First;
                    break;
                case "changing-content": workspace.BeforeContent = () => content.Content = workspace.Second; content.Content = workspace.First; break;
                case "changed-content": workspace.AfterContent = () => content.Content = workspace.Second; content.Content = workspace.First; break;
                case "changing-throw": workspace.BeforeContent = Fail; content.Content = workspace.First; break;
                case "changed-throw": workspace.AfterContent = Fail; content.Content = workspace.First; break;
                case "id-redirect": workspace.AfterIdentifier = () => content.ContentId = "callback"; content.Content = workspace.First; break;
                case "id-throw": workspace.AfterIdentifier = Fail; content.Content = workspace.First; break;
                case "binding-oneway": case "binding-twoway": case "binding-assigned": case "binding-reset": case "binding-explicit":
                    if (operation == "binding-assigned") workspace.Box.Value = "source";
                    workspace.Bind(operation == "binding-oneway" ? BindingMode.OneWay : BindingMode.TwoWay,
                        operation == "binding-explicit");
                    Take("bound", "none");
                    content.Content = workspace.First;
                    Take("content", "none");
                    if (operation == "binding-explicit")
                    {
                        workspace.UpdateSource();
                        Take("updated", "none");
                    }
                    workspace.Box.Value = "replacement";
                    Take("source", "none");
                    if (operation == "binding-reset")
                    {
                        workspace.Box.Value = null;
                        Take("reset", "none");
                        content.Content = workspace.Second;
                    }
                    break;
                case "item-null": case "item-empty":
                    workspace.Item!.ContentId = "item";
                    Take("identifier", "none");
                    workspace.Item.ContentId = operation == "item-null" ? null : "";
                    break;
                case "item-named": content.Content = workspace.First; break;
                case "item-reset":
                    content.Content = workspace.First;
                    Take("content", "none");
                    workspace.Item!.ContentId = null;
                    Take("reset", "none");
                    content.Content = workspace.Second;
                    break;
                case "item-model-id": content.ContentId = "model"; break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
        catch (Exception exception) { error = exception.GetType().Name; }
        Take("after", error);
        return rows.ToArray();

        void Take(string stage, string failure)
        {
            Flush();
            rows.Add(string.Join('\t', scenario, stage, workspace.State(), failure,
                workspace.Events.Count == 0 ? "none" : string.Join('>', workspace.Events)));
            workspace.Events.Clear();
        }
    }

    private static void Fail() => throw new InvalidOperationException("Probe callback");

    private static void Flush()
    {
#if WPF_REFERENCE
        Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.Background);
#else
        Dispatcher.UIThread.RunJobs();
#endif
    }

    private sealed class Workspace : IDisposable
    {
        internal LayoutContent Content { get; }
        internal Border First { get; } = new() { Name = "First" };
        internal Border Second { get; } = new() { Name = "Second" };
        internal EqualValue EqualFirst { get; } = new();
        internal EqualValue EqualSecond { get; } = new();
        internal ValueBox Box { get; } = new();
        internal List<string> Events { get; } = new();
        internal Action? BeforeContent { get; set; }
        internal Action? AfterContent { get; set; }
        internal Action? AfterIdentifier { get; set; }
        internal LayoutItem? Item { get; }
        private readonly DockingManager? _manager;
#if !WPF_REFERENCE
        private IDisposable? _binding;
#endif

        internal Workspace(bool tools, bool item)
        {
            Content = tools ? new LayoutAnchorable() : new LayoutDocument();
            if (item)
            {
                ILayoutPanelElement pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)Content)
                    : new LayoutDocumentPane((LayoutDocument)Content);
                _manager = new DockingManager { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
                Item = _manager.GetLayoutItemFromModel(Content) ?? throw new InvalidOperationException("Missing layout item");
            }
            Content.PropertyChanging += (_, args) =>
            {
                if (args.PropertyName is not ("Content" or "ContentId" or "Id")) return;
                Events.Add("changing:" + args.PropertyName + "@" + State());
                if (args.PropertyName == "Content")
                {
                    Action? action = BeforeContent; BeforeContent = null; action?.Invoke();
                }
            };
            Content.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is not ("Content" or "ContentId" or "Id")) return;
                Events.Add("changed:" + args.PropertyName + "@" + State());
                if (args.PropertyName == "Content")
                {
                    Action? action = AfterContent; AfterContent = null; action?.Invoke();
                }
                else if (args.PropertyName == "ContentId")
                {
                    Action? action = AfterIdentifier; AfterIdentifier = null; action?.Invoke();
                }
            };
        }

        internal string State() => $"id={Text(Content.ContentId)};content={Token(Content.Content)};source={Text(Box.Value)};bound={IsBound()};item={(Item is null ? "absent" : Text(Item.ContentId))}";
        private static string Text(string? value) => value is null ? "null" : value.Length == 0 ? "empty" : value == " " ? "space" : value;
        private string Token(object? value) => value is null ? "null" : ReferenceEquals(value, First) ? "first"
            : ReferenceEquals(value, Second) ? "second" : ReferenceEquals(value, EqualFirst) ? "equal-first"
            : ReferenceEquals(value, EqualSecond) ? "equal-second" : "other";

        private bool IsBound()
        {
#if WPF_REFERENCE
            return BindingOperations.IsDataBound(Content, LayoutContent.ContentIdProperty);
#else
            return BindingOperations.GetBindingExpressionBase(Content, LayoutContent.ContentIdProperty) is not null;
#endif
        }

        internal void Bind(BindingMode mode, bool explicitUpdate)
        {
            UpdateSourceTrigger trigger = explicitUpdate ? UpdateSourceTrigger.Explicit : UpdateSourceTrigger.PropertyChanged;
#if WPF_REFERENCE
            BindingOperations.SetBinding(Content, LayoutContent.ContentIdProperty,
                new Binding { Source = Box, Path = new PropertyPath(ValueBox.ValueProperty), Mode = mode, UpdateSourceTrigger = trigger });
#else
            CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueBox.ValueProperty,
                PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
            _binding = Content.Bind(LayoutContent.ContentIdProperty,
                new CompiledBindingExtension(path) { Source = Box, Mode = mode, UpdateSourceTrigger = trigger });
#endif
        }

        internal void UpdateSource()
        {
#if WPF_REFERENCE
            BindingOperations.GetBindingExpression(Content, LayoutContent.ContentIdProperty).UpdateSource();
#else
            BindingOperations.GetBindingExpressionBase(Content, LayoutContent.ContentIdProperty)!.UpdateSource();
#endif
        }

        public void Dispose()
        {
#if WPF_REFERENCE
            BindingOperations.ClearBinding(Content, LayoutContent.ContentIdProperty);
#else
            _binding?.Dispose();
#endif
            GC.KeepAlive(_manager);
        }
    }

    private sealed class EqualValue
    {
        public override bool Equals(object? obj) => obj is EqualValue;
        public override int GetHashCode() => 1;
    }

#if WPF_REFERENCE
    private sealed class ValueBox : DependencyObject
    {
        internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(ValueBox));
        internal string? Value { get => (string?)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
#else
    private sealed class ValueBox : AvaloniaObject
    {
        internal static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<ValueBox, string?>(nameof(Value));
        internal string? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
#endif
}
