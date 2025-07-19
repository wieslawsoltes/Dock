# Tabs in the Title Bar

`HostWindow` can display document tabs directly in the window chrome. Set `ShowTabsInTitleBar` to `true` so the `ChromeOverlayLayer` hosts a `DocumentTabStrip` next to the caption buttons.

```xaml
<avaloniaDock:HostWindow ShowTabsInTitleBar="True" />
```

Tabs still behave the same as in a regular `DocumentDock`. Use the property when you want a compact layout similar to modern IDEs where the title bar doubles as the tab strip.

