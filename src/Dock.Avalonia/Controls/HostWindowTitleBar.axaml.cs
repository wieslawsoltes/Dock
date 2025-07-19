// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
/// </summary>
[TemplatePart("PART_Background", typeof(Control))]
[TemplatePart("PART_TabStripHost", typeof(ContentPresenter))]
public class HostWindowTitleBar : TitleBar
{
    internal Control? BackgroundControl { get; private set; }
    internal ContentPresenter? TabStripHost { get; private set; }
    private Control? _tabStrip;

    /// <summary>
    /// Gets or sets the tab strip displayed in the title bar.
    /// </summary>
    public Control? TabStrip
    {
        get => _tabStrip;
        set
        {
            _tabStrip = value;
            if (TabStripHost is { })
            {
                TabStripHost.Content = value;
            }
        }
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        BackgroundControl = e.NameScope.Find<Control>("PART_Background");
        TabStripHost = e.NameScope.Find<ContentPresenter>("PART_TabStripHost");
        if (TabStripHost is { })
        {
            TabStripHost.Content = TabStrip;
        }
    }
}
