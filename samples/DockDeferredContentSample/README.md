# DockDeferredContentSample

This sample demonstrates the `Dock.Controls.DeferredContentControl` package directly.

## What it shows

- The shared default deferred timeline.
- A scoped timeline applied through the inheritable `DeferredContentScheduling.Timeline` attached property.
- Per-host `DeferredContentScheduling.Delay`.
- Per-host `DeferredContentScheduling.Order`.
- `DeferredContentControl`.
- `DeferredContentPresenter`.
- A templated presenter-contract host that uses `DeferredContentPresenter` internally.
- Count-based and time-based timeline budgets.

## Run

```bash
dotnet run --project samples/DockDeferredContentSample/DockDeferredContentSample.csproj
```
