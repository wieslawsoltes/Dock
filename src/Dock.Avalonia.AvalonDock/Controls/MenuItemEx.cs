using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AvalonDock.Controls;

/// <summary>A menu item whose icon can be created by a compiled template or selector.</summary>
public class MenuItemEx : MenuItem
{
    /// <summary>Defines the template used to create the icon when there is no selector.</summary>
    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<MenuItemEx, IDataTemplate?>(nameof(IconTemplate));
    /// <summary>Defines the selector that receives the current icon and this menu item.</summary>
    public static readonly StyledProperty<DataTemplateSelector?> IconTemplateSelectorProperty =
        AvaloniaProperty.Register<MenuItemEx, DataTemplateSelector?>(nameof(IconTemplateSelector));

    private bool _updatingIcon;

    /// <summary>Creates a menu item using the current Avalonia menu theme.</summary>
    public MenuItemEx() { }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(MenuItem);
    /// <summary>Gets or sets the icon template.</summary>
    public IDataTemplate? IconTemplate { get => GetValue(IconTemplateProperty); set => SetValue(IconTemplateProperty, value); }
    /// <summary>Gets or sets the icon selector. A selector takes precedence over the template, including null results.</summary>
    public DataTemplateSelector? IconTemplateSelector { get => GetValue(IconTemplateSelectorProperty); set => SetValue(IconTemplateSelectorProperty, value); }

    /// <summary>Recreates the icon after its template changes.</summary>
    protected virtual void OnIconTemplateChanged(AvaloniaPropertyChangedEventArgs e) => UpdateIcon();
    /// <summary>Recreates the icon after its selector changes.</summary>
    protected virtual void OnIconTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => UpdateIcon();

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconTemplateProperty) OnIconTemplateChanged(change);
        else if (change.Property == IconTemplateSelectorProperty) OnIconTemplateSelectorChanged(change);
        else if (change.Property == IconProperty && change.NewValue is not null) UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (_updatingIcon) return;
        _updatingIcon = true;
        try
        {
            IDataTemplate? template = IconTemplateSelector is { } selector ? selector.SelectTemplate(Icon, this) : IconTemplate;
            // WPF LoadContent creates an unbound root. Let the generated icon
            // inherit the menu item's data context when it joins the logical tree.
            if (template is not null) SetCurrentValue(IconProperty, template.Build(null));
        }
        finally { _updatingIcon = false; }
    }
}
