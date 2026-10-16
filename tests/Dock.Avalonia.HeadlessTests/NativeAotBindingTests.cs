using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using DockSystemTextJsonSourceGenAotSample;
using Mvvm = Dock.Model.Mvvm.Controls;

namespace Dock.Avalonia.HeadlessTests;

public class NativeAotBindingTests
{
    [AvaloniaFact]
    public void ProportionalSplitter_Observes_Avalonia_Properties_And_Context_Replacement() =>
        BindingSmokeChecks.ValidateProportionalSplitter(new ProportionalDockSplitter(), new ProportionalDockSplitter());

    [AvaloniaFact]
    public void ProportionalSplitter_Observes_Inpc_Properties_And_Context_Replacement() =>
        BindingSmokeChecks.ValidateProportionalSplitter(new Mvvm.ProportionalDockSplitter(), new Mvvm.ProportionalDockSplitter());

    [AvaloniaFact]
    public void GridSplitter_Maps_Avalonia_Enum_Properties_And_Context_Replacement() =>
        BindingSmokeChecks.ValidateGridSplitter(new GridDockSplitter(), new GridDockSplitter());

    [AvaloniaFact]
    public void GridSplitter_Maps_Inpc_Enum_Properties_And_Context_Replacement() =>
        BindingSmokeChecks.ValidateGridSplitter(new Mvvm.GridDockSplitter(), new Mvvm.GridDockSplitter());

    [AvaloniaFact]
    public void Mdi_Menu_Binds_Commands_And_Updates_Owner_And_Context() => BindingSmokeChecks.ValidateMdiCommands();

    [AvaloniaFact]
    public void Overlay_Services_Bind_Explicit_Interfaces_And_Clear_For_Root_Or_Null_Context() => BindingSmokeChecks.ValidateOverlayServices();

    [AvaloniaFact]
    public void CommandBar_Merging_Preserves_All_Kinds() => BindingSmokeChecks.ValidateCommandBarKinds();
}
