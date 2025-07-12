using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display document switcher content.
/// </summary>
public class DocumentSwitcherWindow : Window
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentSwitcherWindow);
}
