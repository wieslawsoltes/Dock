using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
#if WPF_REFERENCE
using Xceed.Wpf.AvalonDock.Layout;
#else
using AvalonDock.Layout;
#endif

namespace AvalonDock.ReferenceProbes;

// The same public operations run against the separately supplied WPF reference
// and the adapter. No reference implementation or framework internals are used.
internal static class FloatingNotificationProbe
{
    internal static IEnumerable<string> Operations(bool tools)
    {
        foreach (string operation in new[]
        {
            "default", "set", "same", "clear", "replace", "remove", "remove-other", "remove-null",
            "replace-null", "replace-wrong-type", "replace-other", "replace-empty",
            "read-empty", "read-empty-pair", "read-empty-new", "read-empty-pair-new", "read-child", "restore-former"
        }) yield return operation;
        if (!tools) yield break;
        foreach (string operation in new[]
        {
            "compute-empty", "compute-empty-repeat", "compute-visible", "set-empty-group", "set-hidden-group",
            "replace-empty-group", "child-clear", "child-clear-restore", "child-add-pane", "child-remove-pane",
            "read-single-pane", "read-is-single-pane", "nested-single-pane", "read-multiple-pane"
        }) yield return operation;
    }

    internal static void Run(string output)
    {
        List<string> rows = new() { "scenario\tstage\tstate\terror\tevents" };
        foreach (bool tools in new[] { false, true })
        foreach (string suppression in new[] { "none", "changing", "changed" })
        foreach (string operation in Operations(tools))
            rows.AddRange(Observe(tools, suppression, operation));
        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} WPF floating-notification observations.");
    }

    internal static string[] Observe(bool tools, string suppression, string operation)
    {
        LayoutElement first = CreateChild(tools, "First");
        if (operation == "nested-single-pane")
        {
            LayoutAnchorablePaneGroup nested = new();
            nested.Children.Add((LayoutAnchorablePaneGroup)first);
            first = nested;
        }
        LayoutElement second = CreateChild(tools, "Second");
        LayoutFloatingWindow model = tools ? new ToolWindow() : new DocumentWindow();
        bool startsEmpty = operation is "default" or "set" or "replace-empty" or "compute-empty" or
            "compute-empty-repeat" or "set-empty-group" or "set-hidden-group" or "read-empty-new" or "read-empty-pair-new";
        if (!startsEmpty) SetChild(first);
        if (operation is "child-remove-pane" or "read-multiple-pane")
            ((LayoutAnchorablePaneGroup)first).Children.Add(new LayoutAnchorablePane(new LayoutAnchorable()));
        if (model is ToolWindow toolModel) toolModel.Suppression = suppression;
        else ((DocumentWindow)model).Suppression = suppression;

        List<string> events = new();
        model.PropertyChanging += (_, args) => events.Add("changing:" + args.PropertyName + "@" + State());
        model.PropertyChanged += (_, args) => events.Add("changed:" + args.PropertyName + "@" + State());
        ObserveParent(first, "first");
        ObserveParent(second, "second");
        if (model is LayoutDocumentFloatingWindow document)
            document.RootDocumentChanged += (_, _) => events.Add("document-event@" + State());
        if (model is LayoutAnchorableFloatingWindow tool)
            tool.IsVisibleChanged += (_, _) => events.Add("visibility-event@" + State());

        string id = $"{(tools ? "tool" : "document")}-{suppression}-{operation}";
        string before = Row("before", "none");
        string error = "none";
        try
        {
            switch (operation)
            {
                case "default": break;
                case "set": SetChild(first); break;
                case "same": SetChild(first); break;
                case "clear": SetChild(null); break;
                case "replace": SetChild(second); break;
                case "remove": model.RemoveChild(first); break;
                case "remove-other": model.RemoveChild(second); break;
                case "remove-null": model.RemoveChild(null!); break;
                case "replace-null": model.ReplaceChild(first, null!); break;
                case "replace-wrong-type": model.ReplaceChild(first, new LayoutPanel()); break;
                case "replace-other": model.ReplaceChild(second, second); break;
                case "replace-empty": model.ReplaceChild(null!, first); break;
                case "read-empty":
                case "read-empty-new": ReadXml(true, false); break;
                case "read-empty-pair":
                case "read-empty-pair-new": ReadXml(false, false); break;
                case "read-child": ReadXml(false, true); break;
                case "restore-former": SetChild(second); SetChild(first); break;
                case "compute-empty":
                case "compute-visible": ((ILayoutElementWithVisibility)model).ComputeVisibility(); break;
                case "compute-empty-repeat":
                    ((ILayoutElementWithVisibility)model).ComputeVisibility();
                    ((ILayoutElementWithVisibility)model).ComputeVisibility();
                    break;
                case "set-empty-group": SetChild(new LayoutAnchorablePaneGroup()); break;
                case "set-hidden-group":
                    LayoutAnchorablePaneGroup empty = new();
                    empty.ComputeVisibility();
                    SetChild(empty);
                    break;
                case "replace-empty-group": SetChild(new LayoutAnchorablePaneGroup()); break;
                case "child-clear": ((LayoutAnchorablePane)((LayoutAnchorablePaneGroup)first).Children[0]).Children.Clear(); break;
                case "child-clear-restore":
                    LayoutAnchorablePane pane = (LayoutAnchorablePane)((LayoutAnchorablePaneGroup)first).Children[0];
                    LayoutAnchorable content = pane.Children[0];
                    pane.Children.Clear();
                    pane.Children.Add(content);
                    break;
                case "child-add-pane": ((LayoutAnchorablePaneGroup)first).Children.Add(new LayoutAnchorablePane(new LayoutAnchorable())); break;
                case "child-remove-pane": ((LayoutAnchorablePaneGroup)first).Children.RemoveAt(1); break;
                case "read-single-pane":
                case "read-is-single-pane":
                case "nested-single-pane":
                case "read-multiple-pane":
                    foreach (LayoutAnchorablePane childPane in first.Descendents().OfType<LayoutAnchorablePane>())
                        childPane.PropertyChanged += (_, args) =>
                        {
                            if (args.PropertyName == "IsDirectlyHostedInFloatingWindow")
                                events.Add("pane-hosted-changed:" + Flag(childPane.IsDirectlyHostedInFloatingWindow));
                        };
                    LayoutAnchorableFloatingWindow window = (LayoutAnchorableFloatingWindow)model;
                    if (operation == "read-is-single-pane") { _ = window.IsSinglePane; _ = window.IsSinglePane; }
                    else { _ = window.SinglePane; _ = window.SinglePane; }
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
        catch (Exception exception) { error = exception.GetType().Name; }
        return new[] { before, Row("after", error) };

        void SetChild(LayoutElement? child)
        {
            if (model is LayoutAnchorableFloatingWindow tool) tool.RootPanel = (LayoutAnchorablePaneGroup?)child;
            else ((LayoutDocumentFloatingWindow)model).RootDocument = (LayoutDocument?)child;
        }

        void ReadXml(bool shortForm, bool withChild)
        {
            string name = tools ? "LayoutAnchorableFloatingWindow" : "LayoutDocumentFloatingWindow";
            string child = !withChild ? "" : tools
                ? "<LayoutAnchorablePaneGroup><LayoutAnchorablePane><LayoutAnchorable Title=\"Read\" /></LayoutAnchorablePane></LayoutAnchorablePaneGroup>"
                : "<LayoutDocument Title=\"Read\" />";
            using StringReader text = new(shortForm ? $"<{name} />" : $"<{name}>{child}</{name}>");
            using XmlReader reader = XmlReader.Create(text);
            model.ReadXml(reader);
        }

        string State()
        {
            LayoutElement? child = model is LayoutAnchorableFloatingWindow t ? t.RootPanel : ((LayoutDocumentFloatingWindow)model).RootDocument;
            string name = child is null ? "none" : ReferenceEquals(child, first) ? "first" : ReferenceEquals(child, second) ? "second" : "other";
            string visible = model is LayoutAnchorableFloatingWindow tool ? Flag(tool.IsVisible) : "-";
            string single = model is LayoutAnchorableFloatingWindow group ? Flag(group.IsSinglePane) : "-";
            return string.Join(',', name, model.ChildrenCount, model.Children.Count(), Flag(model.IsValid), visible, single,
                Parent(first), Parent(second), child is null ? "-" : Parent(child));
        }

        string Parent(ILayoutElement element) => element.Parent is null ? "none" : ReferenceEquals(element.Parent, model) ? "floating" : "other";
        string Row(string stage, string exception) => string.Join('\t', id, stage, State(), exception, string.Join('>', events));
        void ObserveParent(LayoutElement element, string name)
        {
            element.PropertyChanging += (_, args) =>
            {
                if (args.PropertyName == "Parent") events.Add(name + "-parent-changing@" + State());
            };
            element.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == "Parent") events.Add(name + "-parent-changed@" + State());
            };
        }
    }

    private static string Flag(bool value) => value ? "1" : "0";
    private static LayoutElement CreateChild(bool tools, string title) => tools
        ? new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable { Title = title }))
        : new LayoutDocument { Title = title };

    private sealed class DocumentWindow : LayoutDocumentFloatingWindow
    {
        internal string Suppression { get; set; } = "none";
        protected override void RaisePropertyChanging(string? propertyName = null)
        {
            if (Suppression != "changing") base.RaisePropertyChanging(propertyName);
        }
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (Suppression != "changed") base.RaisePropertyChanged(propertyName);
        }
    }

    private sealed class ToolWindow : LayoutAnchorableFloatingWindow
    {
        internal string Suppression { get; set; } = "none";
        protected override void RaisePropertyChanging(string? propertyName = null)
        {
            if (Suppression != "changing") base.RaisePropertyChanging(propertyName);
        }
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (Suppression != "changed") base.RaisePropertyChanged(propertyName);
        }
    }
}
