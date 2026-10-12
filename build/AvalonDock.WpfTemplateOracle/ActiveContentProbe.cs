using System;
using System.Collections.Generic;
using System.IO;
#if WPF_REFERENCE
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using ChangeArgs = System.Windows.DependencyPropertyChangedEventArgs;
#else
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Styling;
using Avalonia.Threading;
using AvalonDock.Layout;
using ChangeArgs = Avalonia.AvaloniaPropertyChangedEventArgs;
#endif

namespace AvalonDock.ReferenceProbes;

// Shared public operations only: the reference implementation stays outside Dock.
internal static class ActiveContentProbe
{
    internal static IEnumerable<string> Operations() => new[]
    {
        "fresh", "layout-preactive", "payload", "wrapper", "alias-content", "alias-id", "alias-empty",
        "alias-unknown-id", "alias-content-priority", "alias-model-content", "unknown", "null",
        "same-payload", "same-wrapper", "model", "content-active", "model-null", "model-empty",
        "duplicate-payload", "duplicate-wrapper", "duplicate-model", "replace-layout", "replace-active-layout",
        "remove-active", "change-content", "hook-redirect", "event-redirect", "hook-redirect-model",
        "event-redirect-model", "hook-throw", "event-throw", "current-value", "clear-value",
        "binding-default", "binding-oneway", "binding-twoway", "binding-current-value", "binding-clear",
        "style", "style-model", "style-clear", "style-remove", "style-clr"
    };

    internal static void Run(string output)
    {
        List<string> rows = new() { "scenario\tstage\tstate\terror\tevents" };
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in Operations()) rows.AddRange(Observe(suppress, operation));
        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} WPF active-content observations.");
    }

    internal static string[] Observe(bool suppress, string operation)
    {
        using Workspace workspace = new(suppress, operation);
        List<string> rows = new();
        string scenario = (suppress ? "override-" : "base-") + operation;
        Take("before", "none");
        string error = "none";
        try
        {
            TraceManager manager = workspace.Manager;
            LayoutRoot layout = manager.Layout;
            switch (operation)
            {
                case "fresh": case "layout-preactive": break;
                case "payload": manager.ActiveContent = workspace.SecondPayload; break;
                case "wrapper": manager.ActiveContent = workspace.Second; break;
                case "alias-content": manager.ActiveContent = new LayoutDocument { Content = workspace.SecondPayload }; break;
                case "alias-id": manager.ActiveContent = new LayoutDocument { ContentId = "second" }; break;
                case "alias-empty": manager.ActiveContent = new LayoutDocument(); break;
                case "alias-unknown-id": manager.ActiveContent = new LayoutDocument { ContentId = "missing" }; break;
                case "alias-content-priority":
                    manager.ActiveContent = new LayoutDocument { ContentId = "first", Content = workspace.SecondPayload }; break;
                case "alias-model-content": manager.ActiveContent = new LayoutDocument { Content = workspace.Second }; break;
                case "unknown": manager.ActiveContent = workspace.Unknown; break;
                case "null": manager.ActiveContent = null; break;
                case "same-payload": manager.ActiveContent = workspace.FirstPayload; break;
                case "same-wrapper": manager.ActiveContent = workspace.First; break;
                case "model": layout.ActiveContent = workspace.Second; break;
                case "content-active": workspace.Second.IsActive = true; break;
                case "model-null": layout.ActiveContent = null; break;
                case "model-empty": layout.ActiveContent = workspace.Empty; break;
                case "duplicate-payload": manager.ActiveContent = workspace.Second.Content; break;
                case "duplicate-wrapper": manager.ActiveContent = workspace.Second; break;
                case "duplicate-model": layout.ActiveContent = workspace.Second; break;
                case "replace-layout": manager.Layout = new LayoutRoot(); break;
                case "replace-active-layout":
                    LayoutDocument replacement = new() { Content = workspace.Unknown, Title = "Replacement" };
                    LayoutRoot replacementRoot = new() { RootPanel = new LayoutPanel { Children = { new LayoutDocumentPane(replacement) } } };
                    replacementRoot.ActiveContent = replacement;
                    manager.Layout = replacementRoot;
                    break;
                case "remove-active": workspace.Pane.Children.Remove(workspace.First); break;
                case "change-content": workspace.First.Content = workspace.Unknown; break;
                case "hook-redirect": case "event-redirect": case "hook-redirect-model": case "event-redirect-model":
                    Action redirect = () => manager.ActiveContent = workspace.First;
                    if (operation.StartsWith("hook-", StringComparison.Ordinal)) manager.BeforeBase = redirect;
                    else workspace.BeforeEvent = redirect;
                    if (operation.EndsWith("-model", StringComparison.Ordinal)) layout.ActiveContent = workspace.Second;
                    else manager.ActiveContent = workspace.Second;
                    break;
                case "hook-throw": case "event-throw":
                    Action fail = static () => throw new InvalidOperationException("Probe callback");
                    if (operation == "hook-throw") manager.BeforeBase = fail;
                    else workspace.BeforeEvent = fail;
                    manager.ActiveContent = workspace.Second;
                    break;
                case "current-value": manager.SetCurrentValue(DockingManager.ActiveContentProperty, workspace.SecondPayload); break;
                case "clear-value": manager.ClearValue(DockingManager.ActiveContentProperty); break;
                case "binding-default": case "binding-oneway": case "binding-twoway":
                    workspace.Bind(operation == "binding-default" ? BindingMode.Default : operation == "binding-oneway" ? BindingMode.OneWay : BindingMode.TwoWay);
                    Take("bound", "none");
                    layout.ActiveContent = workspace.Second;
                    Take("model", "none");
                    workspace.Box.Value = null;
                    Take("source", "none");
                    manager.ActiveContent = workspace.FirstPayload;
                    Take("local", "none");
                    workspace.Box.Value = workspace.SecondPayload;
                    break;
                case "binding-current-value": case "binding-clear":
                    workspace.Bind(BindingMode.TwoWay);
                    Take("bound", "none");
                    if (operation == "binding-clear") manager.ClearValue(DockingManager.ActiveContentProperty);
                    else manager.SetCurrentValue(DockingManager.ActiveContentProperty, workspace.Second);
                    Take("target", "none");
                    workspace.Box.Value = workspace.Unknown;
                    break;
                case "style": case "style-model": case "style-clear": case "style-remove": case "style-clr":
                    manager.ClearValue(DockingManager.ActiveContentProperty);
                    workspace.ApplyStyle();
                    Take("styled", "none");
                    if (operation == "style-clr") manager.ActiveContent = workspace.SecondPayload;
                    else if (operation != "style") layout.ActiveContent = workspace.Second;
                    Take("model", "none");
                    if (operation == "style-clear") manager.ClearValue(DockingManager.ActiveContentProperty);
                    if (operation == "style-remove") workspace.RemoveStyle();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
        catch (Exception exception) { error = exception.GetType().Name; }
        Take("after", error);
        return rows.ToArray();

        void Take(string stage, string failure)
        {
            Flush();
            rows.Add(string.Join('\t', scenario, stage, workspace.State(), failure, string.Join('>', workspace.Events)));
            workspace.Events.Clear();
        }
    }

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
        internal object FirstPayload { get; } = new();
        internal object SecondPayload { get; } = new();
        internal object Unknown { get; } = new();
        internal LayoutDocument First { get; }
        internal LayoutDocument Second { get; }
        internal LayoutDocument Empty { get; } = new() { ContentId = "empty", Title = "Empty" };
        internal LayoutDocumentPane Pane { get; }
        internal TraceManager Manager { get; }
        internal ValueBox Box { get; } = new();
        internal List<string> Events { get; } = new();
        internal Action? BeforeEvent { get; set; }
#if !WPF_REFERENCE
        private IDisposable? _binding;
#endif

        internal Workspace(bool suppress, string operation)
        {
            First = new() { Content = FirstPayload, ContentId = "first", Title = "First" };
            Second = new() { Content = operation.StartsWith("duplicate-", StringComparison.Ordinal) ? FirstPayload : SecondPayload, ContentId = "second", Title = "Second" };
            Pane = new(First) { Children = { Second, Empty } };
            LayoutRoot root = new() { RootPanel = new LayoutPanel { Children = { Pane } } };
            if (operation == "layout-preactive") root.ActiveContent = First;
            Manager = new() { Layout = root, Suppress = suppress };
            if (operation is not ("fresh" or "layout-preactive"))
            {
                root.ActiveContent = First;
                Manager.ActiveContent = operation == "same-wrapper" ? First : FirstPayload;
            }
            Box.Value = FirstPayload;
            Flush();
            Manager.Record = (stage, oldValue, newValue) => Events.Add(stage + ":" + Token(oldValue) + ":" + Token(newValue) + "@" + State());
            Manager.ActiveContentChanged += (_, _) =>
            {
                Events.Add("event-enter@" + State());
                Action? action = BeforeEvent; BeforeEvent = null; action?.Invoke();
                Events.Add("event-exit@" + State());
            };
        }

        internal string State() => $"value={Token(Manager.ActiveContent)};active={Token(Manager.Layout.ActiveContent)};last={Token(Manager.Layout.LastFocusedDocument)};selected={Flag(First.IsSelected)}{Flag(Second.IsSelected)}{Flag(Empty.IsSelected)};flags={Flag(First.IsActive)}{Flag(Second.IsActive)}{Flag(Empty.IsActive)};source={Token(Box.Value)};bound={IsBound()}";
        private static int Flag(bool value) => value ? 1 : 0;
        private string Token(object? value) => value is null ? "null" : ReferenceEquals(value, First) ? "first-model"
            : ReferenceEquals(value, Second) ? "second-model" : ReferenceEquals(value, Empty) ? "empty-model"
            : ReferenceEquals(value, FirstPayload) ? "first-content" : ReferenceEquals(value, SecondPayload) ? "second-content"
            : ReferenceEquals(value, Unknown) ? "unknown" : value is LayoutContent ? "external-model" : "other";

        private bool IsBound()
        {
#if WPF_REFERENCE
            return BindingOperations.IsDataBound(Manager, DockingManager.ActiveContentProperty);
#else
            return BindingOperations.GetBindingExpressionBase(Manager, DockingManager.ActiveContentProperty) is not null;
#endif
        }

        internal void Bind(BindingMode mode)
        {
#if WPF_REFERENCE
            BindingOperations.SetBinding(Manager, DockingManager.ActiveContentProperty,
                new Binding { Source = Box, Path = new PropertyPath(ValueBox.ValueProperty), Mode = mode });
#else
            CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueBox.ValueProperty,
                PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
            _binding = Manager.Bind(DockingManager.ActiveContentProperty, new CompiledBindingExtension(path) { Source = Box, Mode = mode });
#endif
        }

        internal void ApplyStyle()
        {
#if WPF_REFERENCE
            Manager.Style = new Style(typeof(DockingManager)) { Setters = { new Setter(DockingManager.ActiveContentProperty, FirstPayload) } };
#else
            Manager.Styles.Add(new Style { Setters = { new Setter(DockingManager.ActiveContentProperty, FirstPayload) } });
            Manager.ApplyStyling();
#endif
        }

        internal void RemoveStyle()
        {
#if WPF_REFERENCE
            Manager.Style = null;
#else
            Manager.Styles.Clear();
            Manager.ApplyStyling();
#endif
        }

        public void Dispose()
        {
            Manager.Record = null;
#if WPF_REFERENCE
            BindingOperations.ClearBinding(Manager, DockingManager.ActiveContentProperty);
#else
            _binding?.Dispose();
#endif
        }
    }

    private sealed class TraceManager : DockingManager
    {
        internal bool Suppress { get; init; }
        internal Action<string, object?, object?>? Record { get; set; }
        internal Action? BeforeBase { get; set; }
        protected override void OnActiveContentChanged(ChangeArgs args)
        {
            Record?.Invoke("hook-enter", args.OldValue, args.NewValue);
            Action? action = BeforeBase; BeforeBase = null; action?.Invoke();
            if (!Suppress) base.OnActiveContentChanged(args);
            Record?.Invoke("hook-exit", args.OldValue, args.NewValue);
        }
    }

#if WPF_REFERENCE
    private sealed class ValueBox : DependencyObject
    {
        internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(ValueBox));
        internal object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
#else
    private sealed class ValueBox : AvaloniaObject
    {
        internal static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<ValueBox, object?>(nameof(Value));
        internal object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
#endif
}
