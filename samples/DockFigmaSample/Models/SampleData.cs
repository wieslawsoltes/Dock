using System.Collections.ObjectModel;
using Avalonia.Media;

namespace DockFigmaSample.Models;

public static class SampleData
{
    public static ObservableCollection<LayerItem> CreateLayers() => new()
    {
        new LayerItem("Landing Page", "Frame", 0),
        new LayerItem("Hero", "Group", 1),
        new LayerItem("Headline", "Text", 2),
        new LayerItem("Subhead", "Text", 2),
        new LayerItem("CTA", "Group", 2),
        new LayerItem("Button Primary", "Rectangle", 3),
        new LayerItem("Button Label", "Text", 3),
        new LayerItem("Product Cards", "Group", 1),
        new LayerItem("Card 01", "Frame", 2),
        new LayerItem("Card 02", "Frame", 2),
        new LayerItem("Card 03", "Frame", 2),
        new LayerItem("Footer", "Group", 1, isLocked: true)
    };

    public static ObservableCollection<ComponentItem> CreateComponents() => new()
    {
        new ComponentItem("Button / Primary", "CTA with gradient fill"),
        new ComponentItem("Badge / Status", "Rounded pill badge"),
        new ComponentItem("Card / Pricing", "Card with shadow"),
        new ComponentItem("Input / Search", "Leading icon input"),
        new ComponentItem("Nav / Top", "Logo + actions")
    };

    public static ObservableCollection<AssetSwatch> CreateSwatches() => new()
    {
        new AssetSwatch("Primary", Color.Parse("#3D7EFF")),
        new AssetSwatch("Warm", Color.Parse("#FF8A4C")),
        new AssetSwatch("Mint", Color.Parse("#22C55E")),
        new AssetSwatch("Ink", Color.Parse("#1E2228")),
        new AssetSwatch("Canvas", Color.Parse("#ECEFF3")),
        new AssetSwatch("Lavender", Color.Parse("#9A8CFF"))
    };

    public static ObservableCollection<CommentItem> CreateComments() => new()
    {
        new CommentItem("Ava Fox", "Can we nudge the hero title up 12px?", "2m"),
        new CommentItem("Noah Reed", "The pricing cards feel tight on mobile.", "9m"),
        new CommentItem("Iris Park", "Try a softer background behind the CTA.", "1h")
    };

    public static ObservableCollection<RecentFileItem> CreateRecentFiles() => new()
    {
        new RecentFileItem("Marketing Site", "Nimbus", "Updated 12m ago", Color.Parse("#3D7EFF")),
        new RecentFileItem("Onboarding Flow", "Nimbus", "Updated 2h ago", Color.Parse("#FF8A4C")),
        new RecentFileItem("Design System", "Atlas", "Updated yesterday", Color.Parse("#22C55E")),
        new RecentFileItem("Mobile Kit", "Atlas", "Updated 4d ago", Color.Parse("#F97316"))
    };

    public static ObservableCollection<PrototypeFlowItem> CreatePrototypeFlows() => new()
    {
        new PrototypeFlowItem("On Click", "Navigate", "Pricing Modal"),
        new PrototypeFlowItem("On Hover", "Swap", "Button / Hover"),
        new PrototypeFlowItem("After Delay", "Auto Animate", "Hero Alt")
    };

    public static ObservableCollection<InspectSpecItem> CreateInspectSpecs() => new()
    {
        new InspectSpecItem("Spacing", "24 px"),
        new InspectSpecItem("Padding", "32 px"),
        new InspectSpecItem("Radius", "24 px"),
        new InspectSpecItem("Shadow", "0 24 48 0"),
        new InspectSpecItem("Font", "Space Grotesk / 28")
    };

    public static ObservableCollection<ToolItem> CreateToolbarTools() => new()
    {
        new ToolItem("Move", "V", true),
        new ToolItem("Frame", "F"),
        new ToolItem("Section", "S"),
        new ToolItem("Rectangle", "R"),
        new ToolItem("Ellipse", "O"),
        new ToolItem("Pen", "P"),
        new ToolItem("Text", "T"),
        new ToolItem("Comment", "C")
    };
}
