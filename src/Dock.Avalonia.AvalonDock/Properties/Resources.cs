using System.Globalization;
using System.Resources;

namespace AvalonDock.Properties;

/// <summary>Provides the adapter's localized AvalonDock command labels and hints.</summary>
public class Resources
{
    internal Resources() { }

    /// <summary>Gets the resource manager for the adapter's command strings.</summary>
    public static ResourceManager ResourceManager { get; } = new("AvalonDock.Properties.Resources", typeof(Resources).Assembly);

    /// <summary>Gets or sets the culture used for subsequent resource lookups. Null uses the current UI culture.</summary>
    public static CultureInfo? Culture { get; set; }

    /// <summary>Gets the auto-hide command label.</summary>
    public static string Anchorable_AutoHide => Get(nameof(Anchorable_AutoHide));
    /// <summary>Gets the auto-hide button hint.</summary>
    public static string Anchorable_BtnAutoHide_Hint => Get(nameof(Anchorable_BtnAutoHide_Hint));
    /// <summary>Gets the tool close button hint.</summary>
    public static string Anchorable_BtnClose_Hint => Get(nameof(Anchorable_BtnClose_Hint));
    /// <summary>Gets the tool menu button hint.</summary>
    public static string Anchorable_CxMenu_Hint => Get(nameof(Anchorable_CxMenu_Hint));
    /// <summary>Gets the tool docking command label.</summary>
    public static string Anchorable_Dock => Get(nameof(Anchorable_Dock));
    /// <summary>Gets the label for docking a tool as a document.</summary>
    public static string Anchorable_DockAsDocument => Get(nameof(Anchorable_DockAsDocument));
    /// <summary>Gets the tool floating command label.</summary>
    public static string Anchorable_Float => Get(nameof(Anchorable_Float));
    /// <summary>Gets the tool hide command label.</summary>
    public static string Anchorable_Hide => Get(nameof(Anchorable_Hide));
    /// <summary>Gets the document pin button hint.</summary>
    public static string Document_BtnPinned_Hint => Get(nameof(Document_BtnPinned_Hint));
    /// <summary>Gets the document close command label.</summary>
    public static string Document_Close => Get(nameof(Document_Close));
    /// <summary>Gets the label for closing all documents.</summary>
    public static string Document_CloseAll => Get(nameof(Document_CloseAll));
    /// <summary>Gets the label for closing the other documents.</summary>
    public static string Document_CloseAllButThis => Get(nameof(Document_CloseAllButThis));
    /// <summary>Gets the document menu button hint.</summary>
    public static string Document_CxMenu_Hint => Get(nameof(Document_CxMenu_Hint));
    /// <summary>Gets the document docking command label.</summary>
    public static string Document_DockAsDocument => Get(nameof(Document_DockAsDocument));
    /// <summary>Gets the document floating command label.</summary>
    public static string Document_Float => Get(nameof(Document_Float));
    /// <summary>Gets the label for moving a document to the next tab group.</summary>
    public static string Document_MoveToNextTabGroup => Get(nameof(Document_MoveToNextTabGroup));
    /// <summary>Gets the label for moving a document to the previous tab group.</summary>
    public static string Document_MoveToPreviousTabGroup => Get(nameof(Document_MoveToPreviousTabGroup));
    /// <summary>Gets the label for creating a horizontal document group.</summary>
    public static string Document_NewHorizontalTabGroup => Get(nameof(Document_NewHorizontalTabGroup));
    /// <summary>Gets the label for creating a vertical document group.</summary>
    public static string Document_NewVerticalTabGroup => Get(nameof(Document_NewVerticalTabGroup));
    /// <summary>Gets the maximize-window command label.</summary>
    public static string Window_Maximize => Get(nameof(Window_Maximize));
    /// <summary>Gets the restore-window command label.</summary>
    public static string Window_Restore => Get(nameof(Window_Restore));

    private static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;
}
