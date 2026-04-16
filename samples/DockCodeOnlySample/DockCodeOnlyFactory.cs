using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockCodeOnlySample;

internal sealed class DockCodeOnlyFactory : Factory
{
    private const string DocumentsDockId = "Documents";
    private int _nextDocumentIndex = 2;

    public override IDocumentDock CreateDocumentDock()
    {
        DocumentDock documentDock = new();
        ConfigureDocumentDock(documentDock);
        return documentDock;
    }

    public override IRootDock CreateLayout()
    {
        _nextDocumentIndex = 2;

        DocumentDock documentDock = (DocumentDock)CreateDocumentDock();
        documentDock.Id = DocumentsDockId;
        documentDock.Title = "Documents";

        Document firstDocument = CreateDocumentDockable(
            1,
            "Document 1",
            "This sample keeps document content rebuildable so drag/drop and workspace restore do not depend on moving live controls between hosts.");
        documentDock.VisibleDockables = CreateList<IDockable>(firstDocument);
        documentDock.ActiveDockable = firstDocument;

        Tool leftTool = CreateToolDockable(
            "Tool1",
            "Tool 1",
            "Project explorer style content. This tool can dock left, fill the center, or float into a separate window.");
        leftTool.AllowedDockOperations = DockOperationMask.Left | DockOperationMask.Fill | DockOperationMask.Window;

        Tool bottomTool = CreateToolDockable(
            "Tool2",
            "Output",
            "Build output style content. This tool can dock at the bottom, fill the center, or float into a separate window.");
        bottomTool.AllowedDockOperations = DockOperationMask.Bottom | DockOperationMask.Fill | DockOperationMask.Window;

        ToolDock leftPane = new()
        {
            Id = "LeftPane",
            Title = "Left Pane",
            Alignment = Alignment.Left,
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(leftTool),
            ActiveDockable = leftTool
        };

        ToolDock bottomPane = new()
        {
            Id = "BottomPane",
            Title = "Bottom Pane",
            Alignment = Alignment.Bottom,
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(bottomTool),
            ActiveDockable = bottomTool
        };

        ProportionalDock mainLayout = new()
        {
            Id = "MainLayout",
            Title = "Main Layout",
            Orientation = Dock.Model.Core.Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                leftPane,
                new ProportionalDockSplitter(),
                documentDock,
                new ProportionalDockSplitter(),
                bottomPane),
            ActiveDockable = documentDock
        };

        RootDock rootDock = (RootDock)CreateRootDock();
        rootDock.Id = "Root";
        rootDock.Title = "Root";
        rootDock.IsCollapsable = false;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        rootDock.DefaultDockable = mainLayout;
        rootDock.ActiveDockable = mainLayout;

        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        base.InitLayout(layout);

        foreach (IDockable dockable in EnumerateDockables(layout))
        {
            switch (dockable)
            {
                case DocumentDock documentDock:
                    ConfigureDocumentDock(documentDock);
                    break;
                case Document document when document.Content is null:
                    document.Content = CreateDocumentContent(
                        document.Title,
                        $"Restored content for {document.Title}. The factory rehydrates missing content so floating windows and workspace restores stay usable.");
                    break;
                case Tool tool when tool.Content is null:
                    tool.Content = CreateToolContent(
                        tool.Title,
                        $"Restored content for {tool.Title}. Tools use content factories so they can be rebuilt in a new host after docking operations.");
                    break;
            }
        }

        UpdateNextDocumentIndex(layout);
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockWindow? window = base.CreateWindowFrom(dockable);
        if (window is not null)
        {
            window.Title = "Dock Code-Only Sample";
        }

        return window;
    }

    private void ConfigureDocumentDock(DocumentDock documentDock)
    {
        documentDock.IsCollapsable = true;
        documentDock.CanCreateDocument = true;
        documentDock.EnableWindowDrag = true;
        documentDock.AllowedDropOperations = DockOperationMask.Fill;
        documentDock.DocumentFactory = CreateGeneratedDocument;
    }

    private IDockable CreateGeneratedDocument()
    {
        int index = _nextDocumentIndex++;
        return CreateDocumentDockable(
            index,
            $"Document {index}",
            $"Document {index} uses a content factory instead of a pre-built control instance.");
    }

    private static Document CreateDocumentDockable(int index, string title, string body)
    {
        return new Document
        {
            Id = $"Doc{index}",
            Title = title,
            Content = CreateDocumentContent(title, body)
        };
    }

    private static Tool CreateToolDockable(string id, string title, string description)
    {
        return new Tool
        {
            Id = id,
            Title = title,
            Content = CreateToolContent(title, description)
        };
    }

    private static object CreateDocumentContent(string title, string body)
    {
        return new Func<IServiceProvider, object>(_ =>
        {
            TextBlock header = new()
            {
                Text = title,
                FontWeight = FontWeight.SemiBold
            };

            TextBlock description = new()
            {
                Text = "Code-only sample documents are rendered from a factory-created template so the view can be rebuilt after docking operations.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.8
            };

            TextBox editor = new()
            {
                Text = body,
                AcceptsReturn = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinHeight = 240
            };

            return new Border
            {
                Padding = new Thickness(12),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        header,
                        description,
                        editor
                    }
                }
            };
        });
    }

    private static object CreateToolContent(string title, string description)
    {
        return new Func<IServiceProvider, object>(_ =>
        {
            TextBlock header = new()
            {
                Text = title,
                FontWeight = FontWeight.SemiBold
            };

            TextBlock details = new()
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.8
            };

            TextBlock footer = new()
            {
                Text = "Dock the tool around the layout or float it into a separate window.",
                TextWrapping = TextWrapping.Wrap
            };

            return new Border
            {
                Padding = new Thickness(12),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        header,
                        details,
                        footer
                    }
                }
            };
        });
    }

    private void UpdateNextDocumentIndex(IDockable layout)
    {
        int maxDocumentIndex = 0;

        foreach (IDockable dockable in EnumerateDockables(layout))
        {
            if (dockable is not Document { Id: { } id })
            {
                continue;
            }

            if (!id.StartsWith("Doc", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(id.AsSpan(3), out int parsedIndex) && parsedIndex > maxDocumentIndex)
            {
                maxDocumentIndex = parsedIndex;
            }
        }

        _nextDocumentIndex = Math.Max(_nextDocumentIndex, maxDocumentIndex + 1);
    }

    private static IEnumerable<IDockable> EnumerateDockables(IDockable root)
    {
        HashSet<IDockable> seen = new();
        return EnumerateDockables(root, seen);
    }

    private static IEnumerable<IDockable> EnumerateDockables(IDockable dockable, HashSet<IDockable> seen)
    {
        if (!seen.Add(dockable))
        {
            yield break;
        }

        yield return dockable;

        if (dockable is IRootDock rootDock)
        {
            foreach (IDockable pinned in EnumerateCollection(rootDock.LeftPinnedDockables, seen))
            {
                yield return pinned;
            }

            foreach (IDockable pinned in EnumerateCollection(rootDock.RightPinnedDockables, seen))
            {
                yield return pinned;
            }

            foreach (IDockable pinned in EnumerateCollection(rootDock.TopPinnedDockables, seen))
            {
                yield return pinned;
            }

            foreach (IDockable pinned in EnumerateCollection(rootDock.BottomPinnedDockables, seen))
            {
                yield return pinned;
            }

            foreach (IDockable hidden in EnumerateCollection(rootDock.HiddenDockables, seen))
            {
                yield return hidden;
            }

            if (rootDock.Windows is not null)
            {
                foreach (IDockWindow window in rootDock.Windows)
                {
                    if (window.Layout is null)
                    {
                        continue;
                    }

                    foreach (IDockable windowDockable in EnumerateDockables(window.Layout, seen))
                    {
                        yield return windowDockable;
                    }
                }
            }
        }

        if (dockable is IDock dock && dock.VisibleDockables is not null)
        {
            foreach (IDockable child in dock.VisibleDockables)
            {
                foreach (IDockable descendant in EnumerateDockables(child, seen))
                {
                    yield return descendant;
                }
            }
        }

        if (dockable is ISplitViewDock splitViewDock)
        {
            if (splitViewDock.PaneDockable is { } paneDockable)
            {
                foreach (IDockable descendant in EnumerateDockables(paneDockable, seen))
                {
                    yield return descendant;
                }
            }

            if (splitViewDock.ContentDockable is { } contentDockable)
            {
                foreach (IDockable descendant in EnumerateDockables(contentDockable, seen))
                {
                    yield return descendant;
                }
            }
        }
    }

    private static IEnumerable<IDockable> EnumerateCollection(IList<IDockable>? dockables, HashSet<IDockable> seen)
    {
        if (dockables is null)
        {
            yield break;
        }

        foreach (IDockable dockable in dockables)
        {
            foreach (IDockable descendant in EnumerateDockables(dockable, seen))
            {
                yield return descendant;
            }
        }
    }
}
