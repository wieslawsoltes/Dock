# Overlay customization guide

This guide explains how to customize the overlay stack and control themes used by `OverlayHost`, including custom layers, order changes, and template overrides.

## Default overlay stack
The Fluent theme ships a default layer collection (`DockDefaultOverlayLayers`) that contains:
- Dialog overlay (ZIndex 10)
- Confirmation overlay (ZIndex 20)
- Busy overlay (ZIndex 30)

Each layer uses a style key from `OverlayLayerStyleKeys` so the control theme can be replaced without changing the layer definition.

## Replace the overlay layer stack
Use a custom `OverlayLayerCollection` to change ordering, input blocking, or the overlay controls used.

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <OverlayLayerCollection x:Key="MyOverlayLayers" x:Shared="False">
    <OverlayLayer ZIndex="5"
                  BlocksInput="False"
                  StyleKey="{x:Static OverlayLayerStyleKeys.DialogLayerThemeKey}">
      <DialogOverlayControl DialogService="{Binding Dialogs}"
                            GlobalDialogService="{Binding GlobalDialogService}" />
    </OverlayLayer>
    <OverlayLayer ZIndex="15"
                  StyleKey="{x:Static OverlayLayerStyleKeys.ConfirmationLayerThemeKey}">
      <ConfirmationOverlayControl ConfirmationService="{Binding Confirmations}"
                                  GlobalConfirmationService="{Binding GlobalConfirmationService}" />
    </OverlayLayer>
    <OverlayLayer ZIndex="25"
                  StyleKey="{x:Static OverlayLayerStyleKeys.BusyLayerThemeKey}">
      <BusyOverlayControl BusyService="{Binding Busy}"
                          GlobalBusyService="{Binding GlobalBusyService}" />
    </OverlayLayer>
  </OverlayLayerCollection>

  <ControlTheme x:Key="{x:Type OverlayHost}" TargetType="OverlayHost">
    <Setter Property="OverlayLayers" Value="{DynamicResource MyOverlayLayers}" />
  </ControlTheme>
</ResourceDictionary>
```

Notes:
- Set `x:Shared="False"` for `OverlayLayerCollection` so each host window gets its own instances.
- `BlocksInput="False"` allows pointer input to pass through when the overlay is visible.

## Add a custom overlay layer
Create a custom overlay control and insert it into the stack with a new `OverlayLayer`.

```xml
<OverlayLayerCollection x:Key="MyOverlayLayers" x:Shared="False">
  <OverlayLayer ZIndex="5" BlocksInput="False">
    <MyNotificationOverlay IsVisible="{Binding HasNotifications}" />
  </OverlayLayer>
  <OverlayLayer ZIndex="10"
                StyleKey="{x:Static OverlayLayerStyleKeys.DialogLayerThemeKey}">
    <DialogOverlayControl DialogService="{Binding Dialogs}"
                          GlobalDialogService="{Binding GlobalDialogService}" />
  </OverlayLayer>
  <OverlayLayer ZIndex="20"
                StyleKey="{x:Static OverlayLayerStyleKeys.ConfirmationLayerThemeKey}">
    <ConfirmationOverlayControl ConfirmationService="{Binding Confirmations}"
                                GlobalConfirmationService="{Binding GlobalConfirmationService}" />
  </OverlayLayer>
  <OverlayLayer ZIndex="30"
                StyleKey="{x:Static OverlayLayerStyleKeys.BusyLayerThemeKey}">
    <BusyOverlayControl BusyService="{Binding Busy}"
                        GlobalBusyService="{Binding GlobalBusyService}" />
  </OverlayLayer>
</OverlayLayerCollection>
```

## Override overlay control themes
You can replace the control templates or update visuals via `ControlTheme` resources. The overlay layers reference these keys:
- `OverlayLayerStyleKeys.BusyLayerThemeKey`
- `OverlayLayerStyleKeys.DialogLayerThemeKey`
- `OverlayLayerStyleKeys.ConfirmationLayerThemeKey`

Example: override the busy overlay theme.

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <ControlTheme x:Key="{x:Static OverlayLayerStyleKeys.BusyLayerThemeKey}"
                TargetType="BusyOverlayControl"
                BasedOn="{StaticResource {x:Type BusyOverlayControl}}">
    <Setter Property="Template">
      <ControlTemplate>
        <!-- Custom template markup here. -->
      </ControlTemplate>
    </Setter>
  </ControlTheme>
</ResourceDictionary>
```

## Replace dialog and confirmation shells
Dialog overlays use data templates defined in `OverlayDataTemplates.axaml`. Override them by replacing the resource keys:
- `DockDialogRequestTemplate`
- `DockConfirmationRequestTemplate`

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:services="using:Dock.Model.Services">
  <DataTemplate x:Key="DockDialogRequestTemplate" DataType="services:DialogRequest">
    <MyDialogShell Title="{Binding Title}" Content="{Binding Content}" />
  </DataTemplate>
  <DataTemplate x:Key="DockConfirmationRequestTemplate" DataType="services:ConfirmationRequest">
    <MyConfirmationShell Title="{Binding Title}" Message="{Binding Message}" />
  </DataTemplate>
</ResourceDictionary>
```

## DI-based overlay layer registration
If you want to resolve layers from DI, register `IOverlayLayer` or `IOverlayLayerFactory` implementations and set the registry provider:

```csharp
OverlayLayerRegistry.UseServiceProvider(serviceProvider);
```

Set `OverlayHost.UseServiceLayers="True"` (default) to include DI-provided layers alongside the local `OverlayLayers` collection.

## Visual tree lifecycle
Use `VisualTreeLifecycleBehavior.IsEnabled="True"` on the root `OverlayHost` so overlays rebind when windows or dock controls move in the visual tree.

```xml
<OverlayHost VisualTreeLifecycleBehavior.IsEnabled="True">
  <DockControl Layout="{Binding}" />
</OverlayHost>
```
