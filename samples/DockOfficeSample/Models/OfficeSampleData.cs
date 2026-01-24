using System.Collections.Generic;

namespace DockOfficeSample.Models;

public static class OfficeSampleData
{
    private static readonly IReadOnlyList<OfficeRecentFile> s_recentFiles = new List<OfficeRecentFile>
    {
        new(OfficeAppKind.Word, "LaunchPlan.docx", "Marketing kickoff brief", "Updated 2 hours ago"),
        new(OfficeAppKind.Excel, "Q3Forecast.xlsx", "Finance rollup and scenarios", "Updated yesterday"),
        new(OfficeAppKind.PowerPoint, "BoardDeck.pptx", "Executive summary slides", "Updated last week")
    };

    private static readonly IReadOnlyList<string> s_wordOutlineItems = new List<string>
    {
        "Executive Summary",
        "Objectives",
        "Launch Timeline",
        "Risks and Mitigations",
        "Appendix"
    };

    private static readonly IReadOnlyList<string> s_wordComments = new List<string>
    {
        "Align messaging with sales enablement",
        "Add partner quote",
        "Clarify budget approval"
    };

    private static readonly IReadOnlyList<string> s_wordStyles = new List<string>
    {
        "Heading 1",
        "Heading 2",
        "Subtitle",
        "Pull Quote",
        "Body"
    };

    private static readonly IReadOnlyList<string> s_wordReferences = new List<string>
    {
        "Insert Citation",
        "Table of Contents",
        "Cross-reference",
        "Manage Sources"
    };

    private static readonly IReadOnlyList<string> s_wordReview = new List<string>
    {
        "Track Changes",
        "Accept",
        "Reject",
        "New Comment"
    };

    private static readonly IReadOnlyList<string> s_excelSheets = new List<string>
    {
        "Summary",
        "Revenue",
        "Expenses",
        "Forecast",
        "Scenario"
    };

    private static readonly IReadOnlyList<string> s_excelInsights = new List<string>
    {
        "Revenue up 12% QoQ",
        "Top variance: Region East",
        "OpEx trending down"
    };

    private static readonly IReadOnlyList<string> s_excelFormulas = new List<string>
    {
        "SUM",
        "XLOOKUP",
        "IFERROR",
        "PIVOT"
    };

    private static readonly IReadOnlyList<string> s_excelDataTools = new List<string>
    {
        "Sort",
        "Filter",
        "Remove Duplicates",
        "Data Validation"
    };

    private static readonly IReadOnlyList<string> s_excelCharts = new List<string>
    {
        "Column",
        "Line",
        "Area",
        "Waterfall"
    };

    private static readonly IReadOnlyList<string> s_powerPointSlides = new List<string>
    {
        "Title",
        "Agenda",
        "Market Opportunity",
        "Roadmap",
        "Closing"
    };

    private static readonly IReadOnlyList<string> s_powerPointNotes = new List<string>
    {
        "Rehearse timing",
        "Call out customer quote",
        "Add demo screenshot"
    };

    private static readonly IReadOnlyList<string> s_powerPointDesign = new List<string>
    {
        "Theme Variants",
        "Colors",
        "Fonts",
        "Background"
    };

    private static readonly IReadOnlyList<string> s_powerPointAnimations = new List<string>
    {
        "Appear",
        "Fade",
        "Morph",
        "Zoom"
    };

    private static readonly IReadOnlyList<string> s_powerPointTransitions = new List<string>
    {
        "Fade",
        "Push",
        "Wipe",
        "Morph"
    };

    public static IReadOnlyList<OfficeRecentFile> RecentFiles => s_recentFiles;
    public static IReadOnlyList<string> WordOutlineItems => s_wordOutlineItems;
    public static IReadOnlyList<string> WordComments => s_wordComments;
    public static IReadOnlyList<string> WordStyles => s_wordStyles;
    public static IReadOnlyList<string> WordReferences => s_wordReferences;
    public static IReadOnlyList<string> WordReview => s_wordReview;
    public static IReadOnlyList<string> ExcelSheets => s_excelSheets;
    public static IReadOnlyList<string> ExcelInsights => s_excelInsights;
    public static IReadOnlyList<string> ExcelFormulas => s_excelFormulas;
    public static IReadOnlyList<string> ExcelDataTools => s_excelDataTools;
    public static IReadOnlyList<string> ExcelCharts => s_excelCharts;
    public static IReadOnlyList<string> PowerPointSlides => s_powerPointSlides;
    public static IReadOnlyList<string> PowerPointNotes => s_powerPointNotes;
    public static IReadOnlyList<string> PowerPointDesign => s_powerPointDesign;
    public static IReadOnlyList<string> PowerPointAnimations => s_powerPointAnimations;
    public static IReadOnlyList<string> PowerPointTransitions => s_powerPointTransitions;

    public static IReadOnlyList<string> GetRibbonTabs(OfficeAppKind app)
    {
        return app switch
        {
            OfficeAppKind.Word => new[] { "Home", "Insert", "Layout", "Review", "View" },
            OfficeAppKind.Excel => new[] { "Home", "Insert", "Formulas", "Data", "Review" },
            OfficeAppKind.PowerPoint => new[] { "Home", "Insert", "Design", "Transitions", "Slide Show" },
            _ => new[] { "Home" }
        };
    }

    public static IReadOnlyList<OfficeRibbonGroup> GetRibbonGroups(OfficeAppKind app)
    {
        return app switch
        {
            OfficeAppKind.Word => new[]
            {
                new OfficeRibbonGroup("Clipboard", new[] { "Paste", "Cut", "Copy" }),
                new OfficeRibbonGroup("Font", new[] { "Bold", "Italic", "Underline" }),
                new OfficeRibbonGroup("Paragraph", new[] { "Bullets", "Indent", "Align" })
            },
            OfficeAppKind.Excel => new[]
            {
                new OfficeRibbonGroup("Number", new[] { "Percent", "Currency", "Comma" }),
                new OfficeRibbonGroup("Styles", new[] { "Conditional", "Format" }),
                new OfficeRibbonGroup("Charts", new[] { "Column", "Line", "Pie" })
            },
            OfficeAppKind.PowerPoint => new[]
            {
                new OfficeRibbonGroup("Slides", new[] { "New Slide", "Layout" }),
                new OfficeRibbonGroup("Draw", new[] { "Pen", "Highlighter" }),
                new OfficeRibbonGroup("Arrange", new[] { "Align", "Group" })
            },
            _ => new[] { new OfficeRibbonGroup("Clipboard", new[] { "Paste" }) }
        };
    }
}
