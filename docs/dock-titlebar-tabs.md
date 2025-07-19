# Tabs in the Title Bar

`ShowTabsInTitleBar` allows `DocumentDock` to render its tab strip inside the surrounding window chrome. When combined with `DocumentChromeControlsWholeWindow` the title bar disappears and the document tabs occupy the full width of the window.

## ShowTabsInTitleBar

Set the property on `DocumentDock` (or the `DocumentControl` in Avalonia) to move the tab strip into the title bar area:

```xaml
<avaloniaDock:DocumentDock ShowTabsInTitleBar="True" />
```

This is useful for applications that use custom window chrome or want to maximize vertical space by merging the tabs with the title bar.

## DocumentChromeControlsWholeWindow

`HostWindow` exposes `DocumentChromeControlsWholeWindow` to remove the system chrome completely. When enabled the Dock chrome becomes responsible for the entire window, including the title area.

```xaml
<avaloniaDock:HostWindow DocumentChromeControlsWholeWindow="True" />
```

Enable both properties together to achieve a fully custom title bar with the document tabs rendered at the top of the window.

