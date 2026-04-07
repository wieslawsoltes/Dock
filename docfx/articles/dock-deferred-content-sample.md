# Deferred Content Sample

The deferred content sample is a focused app for `Dock.Controls.DeferredContentControl`.

## Sample project

- `samples/DockDeferredContentSample/DockDeferredContentSample.csproj`
- `samples/DockDeferredContentSample/MainWindow.axaml`

## What it demonstrates

- The shared default timeline configured through `DeferredContentPresentationSettings`.
- A scoped `DeferredContentPresentationTimeline` applied with `DeferredContentScheduling.Timeline`.
- Per-host `DeferredContentScheduling.Delay`.
- Per-host `DeferredContentScheduling.Order`.
- `DeferredContentControl`.
- `DeferredContentPresenter`.
- A templated presenter-contract host that feeds `DeferredContentPresenter` through template bindings.
- Count-based and realization-time budgets.

## Run the sample

```bash
dotnet run --project samples/DockDeferredContentSample/DockDeferredContentSample.csproj
```

## What to validate

- The first group uses the shared default queue.
- The second group realizes in scoped order with per-host delays.
- The third group keeps a `ContentPresenter` contract while using a scoped time-budgeted timeline.
- The first presenter card in the third group appears immediately, while the later cards still stage through the scoped `Delay` and `Order` values.
- The three sections can behave differently because they do not share one queue.
