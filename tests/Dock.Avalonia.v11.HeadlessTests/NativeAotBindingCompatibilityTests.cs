using System;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Mvvm.Controls;
using DockSystemTextJsonSourceGenAotSample;

namespace Dock.Avalonia.v11.HeadlessTests;

public sealed class NativeAotBindingCompatibilityTests
{
    [AvaloniaFact]
    public void ProportionalSplitter_Updates_Without_Reflection() => WithThemes(() =>
        BindingSmokeChecks.ValidateProportionalSplitter(new ProportionalDockSplitter(), new ProportionalDockSplitter()));

    [AvaloniaFact]
    public void GridSplitter_Converts_And_Updates_Without_Reflection() => WithThemes(() =>
        BindingSmokeChecks.ValidateGridSplitter(new GridDockSplitter(), new GridDockSplitter()));

    [AvaloniaFact]
    public void Mdi_Commands_Update_When_Owner_Changes() => WithThemes(BindingSmokeChecks.ValidateMdiCommands);

    [AvaloniaFact]
    public void Overlay_Services_Bind_Through_Their_Interface() => WithThemes(BindingSmokeChecks.ValidateOverlayServices);

    [AvaloniaFact]
    public void CommandBar_Merging_Preserves_All_Kinds() => WithThemes(BindingSmokeChecks.ValidateCommandBarKinds);

    private static void WithThemes(Action check)
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        IStyle[] originalStyles = app.Styles.ToArray();
        try
        {
            app.Styles.Clear();
            app.Styles.Add(new FluentTheme());
            app.Styles.Add(new DockFluentTheme());
            check();
        }
        finally
        {
            app.Styles.Clear();
            foreach (IStyle style in originalStyles)
            {
                app.Styles.Add(style);
            }
        }
    }
}
