# Custom Theme Sample

The `CustomThemeSample` project demonstrates how to load a custom Dock theme and switch between light and dark variants at runtime.

## Key points

- The application defines `AccentLight.axaml` and `AccentDark.axaml` which provide color resources.
- `LightDockTheme.axaml` and `DarkDockTheme.axaml` merge the Dock control styles with the accent resources.
- `App.axaml` loads the light theme by default.
- `CustomThemeManager` swaps the merged style and updates `RequestedThemeVariant` when the **Toggle Theme** button is clicked.

Build and run the sample from the repository root:

```bash
cd samples/CustomThemeSample
dotnet run
```

Press the toggle button to switch themes at runtime.
