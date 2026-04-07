using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace DockDeferredContentSample.ViewModels;

public sealed class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        GlobalSlots =
        [
            CreateSlot(
                "Default A",
                "Uses the shared default timeline from DeferredContentPresentationSettings.",
                0,
                TimeSpan.Zero,
                "Shared default queue with count budgeting.",
                "#4E6AF3"),
            CreateSlot(
                "Default B",
                "Keeps the default order so it follows the first host in FIFO order.",
                0,
                TimeSpan.Zero,
                "Same queue and no scoped overrides.",
                "#1E88E5"),
            CreateSlot(
                "Default C",
                "Still resolves through the global timeline and follows the same follow-up cadence.",
                0,
                TimeSpan.Zero,
                "No scoped timeline, no scoped delay.",
                "#00897B")
        ];

        OrderedSlots =
        [
            CreateSlot(
                "Order 20",
                "This host stays later in the shared scope because its order is larger.",
                20,
                TimeSpan.FromMilliseconds(120),
                "Scoped queue. Lower order wins.",
                "#8E24AA"),
            CreateSlot(
                "Order -10",
                "This host jumps ahead of later items inside the same scoped timeline.",
                -10,
                TimeSpan.Zero,
                "Same scope, earlier order, no extra delay.",
                "#D81B60"),
            CreateSlot(
                "Order 5",
                "This host lands between the negative and high positive orders.",
                5,
                TimeSpan.FromMilliseconds(40),
                "Scoped queue with per-host delay override.",
                "#FB8C00")
        ];

        PresenterSlots =
        [
            CreateSlot(
                "Presenter -5",
                "Realizes first in the scoped presenter queue because it combines the lowest order with no extra delay.",
                -5,
                TimeSpan.Zero,
                "Lower order first, then FIFO for ties.",
                "#00838F"),
            CreateSlot(
                "Presenter 0",
                "Uses DeferredContentPresenter for templates that must keep a ContentPresenter contract and adds a short per-host delay.",
                0,
                TimeSpan.FromMilliseconds(30),
                "Time-budget scope with a short per-host delay.",
                "#3949AB"),
            CreateSlot(
                "Presenter 10",
                "Runs later inside the same scoped presenter queue because the order is larger.",
                10,
                TimeSpan.FromMilliseconds(90),
                "Same time-budget scope, later order.",
                "#6D4C41")
        ];
    }

    public string Overview { get; } =
        "This sample demonstrates the deferred content package without Dock theme integration noise. " +
        "The first section uses the shared default timeline. The second section scopes a custom timeline " +
        "to a subtree and overrides per-host order and delay. The last section keeps the ContentPresenter " +
        "contract and uses a time-budgeted scoped timeline.";

    public string GlobalSummary { get; } =
        "Default behavior stays unchanged: one shared queue, next-pass scheduling, and FIFO ordering when no scoped overrides are supplied.";

    public string OrderedSummary { get; } =
        "The border hosts an inherited timeline. Each child keeps that queue but overrides Delay and Order so the same scope can realize in a controlled sequence.";

    public string PresenterSummary { get; } =
        "This scope uses DeferredContentPresenter instead of DeferredContentControl and switches the queue to realization-time budgeting. The first card has no extra delay so the scope does not look blank on first paint, while later cards still stage through order and delay.";

    public IReadOnlyList<DeferredSampleSlot> GlobalSlots { get; }

    public IReadOnlyList<DeferredSampleSlot> OrderedSlots { get; }

    public IReadOnlyList<DeferredSampleSlot> PresenterSlots { get; }

    private static DeferredSampleSlot CreateSlot(
        string name,
        string hint,
        int order,
        TimeSpan delay,
        string notes,
        string accent)
    {
        return new DeferredSampleSlot(
            name,
            hint,
            order,
            delay,
            $"order={order} delay={delay.TotalMilliseconds:0}ms",
            new DeferredSampleCard(
                $"{name} Realized",
                $"Scope order {order}",
                notes,
                new SolidColorBrush(Color.Parse(accent))));
    }
}

public sealed record DeferredSampleSlot(
    string Name,
    string Hint,
    int Order,
    TimeSpan Delay,
    string Scheduling,
    DeferredSampleCard Card);

public sealed record DeferredSampleCard(
    string Title,
    string Scope,
    string Notes,
    IBrush AccentBrush);
