using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace DockExternalTabStripsSample;

public sealed class MainViewModel
{
    public DockWorkspace LeftWorkspace { get; }

    public DockWorkspace RightWorkspace { get; }

    public MainViewModel()
    {
        LeftWorkspace = CreateWorkspace("Left", "Left");
        RightWorkspace = CreateWorkspace("Right", "Right");
    }

    private static DockWorkspace CreateWorkspace(string workspaceId, string workspaceTitle)
    {
        var factory = new Factory();

        factory.Document(out var primaryDocument, d => d
                   .WithId($"{workspaceId}.Doc1")
                   .WithTitle($"{workspaceTitle}.Main.cs"))
               .Document(out var secondaryDocument, d => d
                   .WithId($"{workspaceId}.Doc2")
                   .WithTitle($"{workspaceTitle}.Readme.md"))
               .DocumentDock(out var documentDock, d => d
                   .WithId($"{workspaceId}.Documents")
                   .WithTitle($"{workspaceTitle} Documents")
                   .WithIsCollapsable(false)
                   .WithEnableWindowDrag(true)
                   .WithCanCreateDocument(true)
                   .WithTabsLayout(DocumentTabLayout.Top)
                   .AppendDocument(primaryDocument)
                   .AppendDocument(secondaryDocument)
                   .WithActiveDockable(primaryDocument)
                   .WithProportion(1.0))
               .RootDock(out var rootDock, r => r
                   .WithId($"{workspaceId}.Root")
                   .WithTitle(workspaceTitle)
                   .Add(documentDock)
                   .WithDefaultDockable(documentDock)
                   .WithActiveDockable(documentDock)
                   .WithEnableGlobalDocking(true));

        if (documentDock is DocumentDock documentDockImpl)
        {
            var nextDocumentIndex = (documentDock.VisibleDockables?.Count ?? 0) + 1;
            documentDockImpl.DocumentFactory = () =>
            {
                var index = nextDocumentIndex++;
                return new Document
                {
                    Id = $"{workspaceId}.Doc{index}",
                    Title = $"{workspaceTitle}.New{index}.txt"
                };
            };
        }

        factory.InitLayout(rootDock);

        return new DockWorkspace(factory, rootDock, documentDock);
    }
}

public sealed class DockWorkspace
{
    public DockWorkspace(IFactory factory, IRootDock layout, IDocumentDock documents)
    {
        Factory = factory;
        Layout = layout;
        Documents = documents;
    }

    public IFactory Factory { get; }

    public IRootDock Layout { get; }

    public IDocumentDock Documents { get; }
}
