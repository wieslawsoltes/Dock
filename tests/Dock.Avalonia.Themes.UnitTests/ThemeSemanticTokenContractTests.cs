using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ThemeSemanticTokenContractTests
{
    private static readonly string[] SemanticTokenKeys =
    [
        "DockSurfaceWorkbenchBrush",
        "DockSurfaceSidebarBrush",
        "DockSurfaceEditorBrush",
        "DockSurfacePanelBrush",
        "DockSurfaceHeaderBrush",
        "DockSurfaceHeaderActiveBrush",
        "DockBorderSubtleBrush",
        "DockBorderStrongBrush",
        "DockSeparatorBrush",
        "DockSplitterIdleBrush",
        "DockSplitterHoverBrush",
        "DockSplitterDragBrush",
        "DockTabBackgroundBrush",
        "DockTabHoverBackgroundBrush",
        "DockTabActiveBackgroundBrush",
        "DockTabActiveIndicatorBrush",
        "DockTabForegroundBrush",
        "DockTabSelectedForegroundBrush",
        "DockTabActiveForegroundBrush",
        "DockTabCloseHoverBackgroundBrush",
        "DockTargetIndicatorBrush",
        "DockChromeButtonForegroundBrush",
        "DockChromeButtonHoverBackgroundBrush",
        "DockChromeButtonPressedBackgroundBrush",
        "DockChromeButtonDangerHoverBrush",
        "DockCornerRadiusSmall",
        "DockHeaderHeight",
        "DockTabHeight",
        "DockTabHorizontalPadding",
        "DockIconSizeSmall",
        "DockIconSizeNormal",
        "DockTabItemMinHeight",
        "DockToolTabItemMinHeight",
        "DockDocumentTabItemPadding",
        "DockToolTabItemPadding",
        "DockToolTabItemSelectedPadding",
        "DockCreateButtonWidth",
        "DockCreateButtonHeight",
        "DockCloseButtonSize",
        "DockChromeButtonWidth",
        "DockChromeButtonHeight",
        "DockChromeButtonPadding",
        "DockChromeButtonMargin",
        "DockMdiTitleIconSize",
        "DockMdiHeaderDragPadding",
        "DockHeaderContentPadding",
        "DockModifiedIndicatorMargin",
        "DockTabContentSpacing",
        "DockDocumentTabHeaderContentSpacing",
        "DockTabContentMargin",
        "DockCreateButtonIconMargin",
        "DockToolChromeHeaderMargin",
        "DockToolChromeTitleMargin",
        "DockToolChromeMenuIconMargin",
        "DockToolChromeDividerThickness",
        "DockChromeGripHeight",
        "DockChromeGripMargin",
        "DockChromeGripBrush",
        "DockMdiHeaderColumnSpacing",
        "DockMdiButtonStripSpacing",
        "DockMdiButtonStripMargin",
        "DockMdiResizeEdgeThickness",
        "DockMdiResizeCornerSize",
        "DockTargetSelectorSize",
        "DockTargetSelectorGridMaxSize",
        "DockSelectorOverlayBackdropBrush",
        "DockSelectorOverlayCornerRadius",
        "DockSelectorOverlayPadding",
        "DockSelectorOverlayMinWidth",
        "DockSelectorOverlayMaxWidth",
        "DockSelectorOverlaySpacing",
        "DockSelectorOverlayListMinHeight",
        "DockSelectorOverlayListMaxHeight",
        "DockSelectorOverlayItemPadding",
        "DockSelectorOverlayItemCornerRadius",
        "DockSelectorOverlayBadgeSpacing",
        "DockSelectorOverlayBadgeMargin",
        "DockSelectorOverlayBadgeCornerRadius",
        "DockSelectorOverlayBadgePadding",
        "DockSelectorOverlayBadgeFontSize",
        "DockCommandBarPadding",
        "DockCommandBarSpacing",
        "DockDragPreviewCornerRadius",
        "DockDragPreviewHeaderPadding",
        "DockDragPreviewHeaderSpacing",
        "DockDragPreviewStatusSpacing",
        "DockDragPreviewStatusIconSize",
        "DockHostTitleBarMouseTrackerHeight",
        "DockOverlayReloadButtonMargin",
        "DockOverlayCardCornerRadius",
        "DockOverlayCardPadding",
        "DockOverlayCardSpacing",
        "DockOverlayMessageFontSize",
        "DockOverlayProgressWidth",
        "DockOverlayProgressHeight",
        "DockDialogCornerRadius",
        "DockDialogPadding",
        "DockDialogMinWidth",
        "DockDialogMaxWidth",
        "DockDialogSpacing",
        "DockDialogTitleFontSize",
        "DockDialogCloseButtonSize",
        "DockConfirmationDialogPadding",
        "DockConfirmationDialogMaxWidth",
        "DockConfirmationDialogStackSpacing",
        "DockConfirmationDialogActionsSpacing"
    ];

    [AvaloniaFact]
    public void DockFluentTheme_Should_Resolve_All_Semantic_Tokens()
    {
        AssertSemanticTokenContract(new DockFluentTheme());
    }

    [AvaloniaFact]
    public void DockSimpleTheme_Should_Resolve_All_Semantic_Tokens()
    {
        AssertSemanticTokenContract(new DockSimpleTheme());
    }

    private static void AssertSemanticTokenContract(Styles theme)
    {
        var app = Application.Current ?? throw new System.InvalidOperationException("Avalonia application is not initialized.");
        List<IStyle> previousStyles = app.Styles.ToList();
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = new Border()
        };

        app.Styles.Clear();
        app.Styles.Add(theme);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var host = (Border)window.Content!;

            foreach (var key in SemanticTokenKeys)
            {
                Assert.True(host.TryFindResource(key, out var value), $"Missing semantic token '{key}'.");
                Assert.NotNull(value);
            }
        }
        finally
        {
            window.Close();
            app.Styles.Clear();
            foreach (var style in previousStyles)
            {
                app.Styles.Add(style);
            }
        }
    }
}
