using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingTemplateTests
{
    [AvaloniaFact]
    public void ViewModel_Content_Renders_In_Floating_Hosts_And_Survives_Xml_Restore()
    {
        SourceViewModel viewModel = new();
        SourcesFixture view = new() { DataContext = viewModel };
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            document.Float();
            tool.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, manager.Layout.FloatingWindows.Count);
            AssertRendered(manager, viewModel.Documents[0], "manager-template");
            AssertRendered(manager, viewModel.Tools[0], "manager-template");
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            Dispatcher.UIThread.RunJobs();
            AssertRendered(manager, viewModel.Documents[0], "manager-template");
            AssertRendered(manager, viewModel.Tools[0], "manager-template");
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    [AvaloniaFact]
    public void Floating_Content_Uses_Current_Template_Scope_And_Does_Not_Recycle_A_Different_Template()
    {
        SourcesFixture view = new() { DataContext = new SourceViewModel() };
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            document.Float();
            Dispatcher.UIThread.RunJobs();
            AssertRendered(manager, (SourceContent)document.Content!, "manager-template");
            IDataTemplate managerTemplate = manager.DataTemplates[0];
            manager.DataTemplates.Clear();
            SourceContent inheritedContent = new("From the window template");
            document.Content = inheritedContent;
            Dispatcher.UIThread.RunJobs();
            AssertRendered(manager, inheritedContent, "window-template");
            manager.DataTemplates.Add(managerTemplate);
            SourceContent localContent = new("From the manager template again");
            document.Content = localContent;
            Dispatcher.UIThread.RunJobs();
            AssertRendered(manager, localContent, "manager-template");
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    [AvaloniaFact]
    public void Host_Factory_And_Template_Proxy_Do_Not_Retain_An_Unused_Manager()
    {
        (Func<IHostWindow?> factory, IHostWindow host, IDataTemplate template, WeakReference reference) = CreateDetachedHost();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(reference.IsAlive);
        Assert.Null(factory());
        Assert.False(template.Match(new SourceContent("Detached")));
        Assert.Null(template.Build(new SourceContent("Detached")));
        host.Exit();
        GC.KeepAlive(factory);
        GC.KeepAlive(template);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Func<IHostWindow?>, IHostWindow, IDataTemplate, WeakReference) CreateDetachedHost()
    {
        DockingManager manager = new();
        Func<IHostWindow?> factory = manager.DockHostWindowFactory;
        HostWindow host = Assert.IsAssignableFrom<HostWindow>(factory());
        return (factory, host, Assert.Single(host.DataTemplates), new WeakReference(manager));
    }

    private static void AssertRendered(DockingManager manager, SourceContent content, string templateClass)
    {
        LayoutFloatingWindow window = manager.Layout.FloatingWindows.Single(x =>
            x.Descendents().OfType<LayoutContent>().Any(item => ReferenceEquals(item.Content, content)));
        HostWindow host = Assert.IsAssignableFrom<HostWindow>(window.DockWindow.Host);
        Assert.True(host.IsVisible);
        Assert.Contains(host.GetVisualDescendants().OfType<TextBlock>(), text =>
            ReferenceEquals(text.DataContext, content) && text.Text == content.Text &&
            text.Classes.Contains(templateClass) && text.Bounds.Width > 0);
    }
}
