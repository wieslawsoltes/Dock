using Dock.Model.Controls;
using Dock.Model.Inpc.Controls;
using Dock.Model.Inpc.Core;

namespace Dock.Serializer.SystemTextJson.SourceGenSharedTypes;

public class LibraryRootDock : RootDock
{
    public string? RootTag { get; set; }
}

public class LibraryDocumentDock : DocumentDock
{
    public string? DockTag { get; set; }
}

public class LibraryToolDock : ToolDock
{
    public string? DockTag { get; set; }
}

public class LibraryDocument : Document
{
    public string? DocumentTag { get; set; }
}

public class LibraryTool : Tool
{
    public string? ToolTag { get; set; }
}

public class LibraryDockWindow : DockWindow
{
    public string? WindowTag { get; set; }
}

public sealed class LibraryDocumentTemplate : IDocumentTemplate
{
    public object? Content { get; set; }

    public string? TemplateTag { get; set; }
}

public sealed class LibraryRegisteredPayload
{
    public string? Name { get; set; }
}
