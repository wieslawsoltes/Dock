using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;

namespace Dock.Controls.Recycling.UnitTests.Controls;

/// <summary>
/// A minimal <see cref="ContentControl"/> whose template presenter does not
/// TemplateBind Content/ContentTemplate from the host
/// </summary>
internal sealed class ManualPresenterHost : ContentControl
{
    static ManualPresenterHost()
    {
        TemplateProperty.OverrideDefaultValue<ManualPresenterHost>(
            new FuncControlTemplate((_, ns) =>
                new ContentPresenter { Name = "PART_ContentPresenter" }.RegisterInNameScope(ns)));
    }
}
