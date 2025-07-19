# WindowChromeSample

`WindowChromeSample` demonstrates how to combine a custom host window chrome with `EnableWindowDrag`. The application uses the `HostWindow` control from Dock and sets `DocumentChromeControlsWholeWindow` so Dock draws the entire chrome.

The `DocumentDock` inside the layout has `EnableWindowDrag` enabled which means the empty area of the tab strip acts as a drag handle for the window.

Run the sample from the repository root:

```bash
dotnet run --project samples/WindowChromeSample
```

Drag anywhere on the document tab bar to move the window.

