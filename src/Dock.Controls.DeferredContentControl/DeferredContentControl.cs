// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.Visuals;

namespace Dock.Controls.DeferredContentControl;

/// <summary>
/// Defines whether a content instance should bypass deferred presentation.
/// </summary>
public interface IDeferredContentPresentation
{
    /// <summary>
    /// Gets a value indicating whether content presentation should be deferred.
    /// </summary>
    bool DeferContentPresentation { get; }
}

/// <summary>
/// Defines how a deferred timeline limits work done in a single dispatcher pass.
/// </summary>
public enum DeferredContentPresentationBudgetMode
{
    /// <summary>
    /// Limits work by the number of realized targets in a single dispatcher pass.
    /// </summary>
    ItemCount,

    /// <summary>
    /// Limits work by elapsed realization time in a single dispatcher pass.
    /// </summary>
    RealizationTime
}

/// <summary>
/// Defines a reusable deferred presentation timeline shared by one or more deferred hosts.
/// </summary>
public sealed class DeferredContentPresentationTimeline
{
    private readonly DeferredContentPresentationTimelineQueue _queue;
    private DeferredContentPresentationBudgetMode _budgetMode = DeferredContentPresentationBudgetMode.ItemCount;
    private int _maxPresentationsPerPass = 8;
    private TimeSpan _maxRealizationTimePerPass = TimeSpan.FromMilliseconds(10);
    private TimeSpan _initialDelay = TimeSpan.Zero;
    private TimeSpan _followUpDelay = TimeSpan.FromMilliseconds(1);
    private TimeSpan _revealDuration = TimeSpan.FromMilliseconds(90);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredContentPresentationTimeline"/> class.
    /// </summary>
    public DeferredContentPresentationTimeline()
    {
        _queue = new DeferredContentPresentationTimelineQueue(this);
    }

    /// <summary>
    /// Gets or sets the active budget mode for the timeline queue.
    /// </summary>
    public DeferredContentPresentationBudgetMode BudgetMode
    {
        get => _budgetMode;
        set => _budgetMode = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of successful realizations allowed in one dispatcher pass when <see cref="BudgetMode"/> is <see cref="DeferredContentPresentationBudgetMode.ItemCount"/>.
    /// </summary>
    public int MaxPresentationsPerPass
    {
        get => _maxPresentationsPerPass;
        set => _maxPresentationsPerPass = value > 0 ? value : 1;
    }

    /// <summary>
    /// Gets or sets the maximum elapsed realization time allowed in one dispatcher pass when <see cref="BudgetMode"/> is <see cref="DeferredContentPresentationBudgetMode.RealizationTime"/>.
    /// </summary>
    public TimeSpan MaxRealizationTimePerPass
    {
        get => _maxRealizationTimePerPass;
        set => _maxRealizationTimePerPass = value >= TimeSpan.Zero ? value : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets or sets the delay applied when a target first joins the timeline.
    /// </summary>
    public TimeSpan InitialDelay
    {
        get => _initialDelay;
        set => _initialDelay = value >= TimeSpan.Zero ? value : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets or sets the delay used between follow-up dispatcher passes while the timeline still has pending due targets.
    /// </summary>
    public TimeSpan FollowUpDelay
    {
        get => _followUpDelay;
        set => _followUpDelay = value >= TimeSpan.Zero ? value : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets or sets the short opacity reveal duration applied when deferred content becomes visible. Set to zero to disable the reveal animation.
    /// </summary>
    public TimeSpan RevealDuration
    {
        get => _revealDuration;
        set => _revealDuration = value >= TimeSpan.Zero ? value : TimeSpan.Zero;
    }

    internal bool AutoSchedule
    {
        get => _queue.AutoSchedule;
        set => _queue.AutoSchedule = value;
    }

    internal int PendingCount => _queue.PendingCount;

    internal void Enqueue(IDeferredContentPresentationTarget target, TimeSpan delay, int order)
    {
        _queue.Enqueue(target, delay, order);
    }

    internal void Remove(IDeferredContentPresentationTarget target)
    {
        _queue.Remove(target);
    }

    internal void FlushPendingBatchForTesting()
    {
        _queue.FlushPendingBatchForTesting();
    }
}

/// <summary>
/// Configures the shared default deferred presentation timeline used when no scoped timeline is supplied.
/// </summary>
public static class DeferredContentPresentationSettings
{
    private static readonly DeferredContentPresentationTimeline s_defaultTimeline = new();

    /// <summary>
    /// Gets the shared default timeline used by deferred hosts that do not resolve a scoped timeline.
    /// </summary>
    public static DeferredContentPresentationTimeline DefaultTimeline => s_defaultTimeline;

    /// <summary>
    /// Gets or sets the active budget mode for the shared default timeline.
    /// </summary>
    public static DeferredContentPresentationBudgetMode BudgetMode
    {
        get => s_defaultTimeline.BudgetMode;
        set => s_defaultTimeline.BudgetMode = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of successful realizations allowed in one dispatcher pass when <see cref="BudgetMode"/> is <see cref="DeferredContentPresentationBudgetMode.ItemCount"/>.
    /// </summary>
    public static int MaxPresentationsPerPass
    {
        get => s_defaultTimeline.MaxPresentationsPerPass;
        set => s_defaultTimeline.MaxPresentationsPerPass = value;
    }

    /// <summary>
    /// Gets or sets the maximum elapsed realization time allowed in one dispatcher pass when <see cref="BudgetMode"/> is <see cref="DeferredContentPresentationBudgetMode.RealizationTime"/>.
    /// </summary>
    public static TimeSpan MaxRealizationTimePerPass
    {
        get => s_defaultTimeline.MaxRealizationTimePerPass;
        set => s_defaultTimeline.MaxRealizationTimePerPass = value;
    }

    /// <summary>
    /// Gets or sets the initial delay applied when a target first joins the shared default timeline.
    /// </summary>
    public static TimeSpan InitialDelay
    {
        get => s_defaultTimeline.InitialDelay;
        set => s_defaultTimeline.InitialDelay = value;
    }

    /// <summary>
    /// Gets or sets the delay used between follow-up dispatcher passes while the shared default timeline still has pending due targets.
    /// </summary>
    public static TimeSpan FollowUpDelay
    {
        get => s_defaultTimeline.FollowUpDelay;
        set => s_defaultTimeline.FollowUpDelay = value;
    }

    /// <summary>
    /// Gets or sets the short opacity reveal duration applied when deferred content becomes visible through the shared default timeline. Set to zero to disable the reveal animation.
    /// </summary>
    public static TimeSpan RevealDuration
    {
        get => s_defaultTimeline.RevealDuration;
        set => s_defaultTimeline.RevealDuration = value;
    }
}

/// <summary>
/// Provides inheritable attached properties that scope deferred presentation timelines and per-host scheduling metadata.
/// </summary>
public sealed class DeferredContentScheduling
{
    /// <summary>
    /// Defines the deferred presentation timeline attached property.
    /// </summary>
    public static readonly AttachedProperty<DeferredContentPresentationTimeline?> TimelineProperty =
        AvaloniaProperty.RegisterAttached<DeferredContentScheduling, AvaloniaObject, DeferredContentPresentationTimeline?>(
            "Timeline",
            defaultValue: null,
            inherits: true);

    /// <summary>
    /// Defines the per-host deferred delay attached property.
    /// </summary>
    public static readonly AttachedProperty<TimeSpan> DelayProperty =
        AvaloniaProperty.RegisterAttached<DeferredContentScheduling, AvaloniaObject, TimeSpan>(
            "Delay",
            defaultValue: TimeSpan.Zero,
            inherits: true);

    /// <summary>
    /// Defines the per-host deferred order attached property.
    /// </summary>
    public static readonly AttachedProperty<int> OrderProperty =
        AvaloniaProperty.RegisterAttached<DeferredContentScheduling, AvaloniaObject, int>(
            "Order",
            defaultValue: 0,
            inherits: true);

    private DeferredContentScheduling()
    {
    }

    /// <summary>
    /// Gets the deferred presentation timeline for the specified object.
    /// </summary>
    public static DeferredContentPresentationTimeline? GetTimeline(AvaloniaObject target)
    {
        return target.GetValue(TimelineProperty);
    }

    /// <summary>
    /// Sets the deferred presentation timeline for the specified object.
    /// </summary>
    public static void SetTimeline(AvaloniaObject target, DeferredContentPresentationTimeline? value)
    {
        target.SetValue(TimelineProperty, value);
    }

    /// <summary>
    /// Gets the deferred delay for the specified object.
    /// </summary>
    public static TimeSpan GetDelay(AvaloniaObject target)
    {
        return target.GetValue(DelayProperty);
    }

    /// <summary>
    /// Sets the deferred delay for the specified object.
    /// </summary>
    public static void SetDelay(AvaloniaObject target, TimeSpan value)
    {
        target.SetValue(DelayProperty, value);
    }

    /// <summary>
    /// Gets the deferred order for the specified object.
    /// </summary>
    public static int GetOrder(AvaloniaObject target)
    {
        return target.GetValue(OrderProperty);
    }

    /// <summary>
    /// Sets the deferred order for the specified object.
    /// </summary>
    public static void SetOrder(AvaloniaObject target, int value)
    {
        target.SetValue(OrderProperty, value);
    }
}

/// <summary>
/// A <see cref="ContentControl"/> that batches content materialization onto a deferred timeline.
/// </summary>
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class DeferredContentControl : ContentControl, IDeferredContentPresentationTarget
{
    private ContentPresenter? _presenter;
    private object? _appliedContent;
    private IDataTemplate? _appliedContentTemplate;
    private long _requestedVersion;
    private long _appliedVersion = -1;
    private DeferredContentPresentationTimeline? _enqueuedTimeline;

    static DeferredContentControl()
    {
        TemplateProperty.OverrideDefaultValue<DeferredContentControl>(
            new FuncControlTemplate((_, nameScope) => new DeferredContentPresenter
            {
                Name = "PART_ContentPresenter",
                [~ContentPresenter.BackgroundProperty] = new TemplateBinding(BackgroundProperty),
                [~ContentPresenter.BackgroundSizingProperty] = new TemplateBinding(BackgroundSizingProperty),
                [~ContentPresenter.BorderBrushProperty] = new TemplateBinding(BorderBrushProperty),
                [~ContentPresenter.BorderThicknessProperty] = new TemplateBinding(BorderThicknessProperty),
                [~ContentPresenter.CornerRadiusProperty] = new TemplateBinding(CornerRadiusProperty),
                [~ContentPresenter.PaddingProperty] = new TemplateBinding(PaddingProperty),
                [~ContentPresenter.VerticalContentAlignmentProperty] = new TemplateBinding(VerticalContentAlignmentProperty),
                [~ContentPresenter.HorizontalContentAlignmentProperty] = new TemplateBinding(HorizontalContentAlignmentProperty)
            }.RegisterInNameScope(nameScope)));
    }

    DeferredContentPresentationTimeline? IDeferredContentPresentationTarget.EnqueuedTimeline
    {
        get => _enqueuedTimeline;
        set => _enqueuedTimeline = value;
    }

    bool IDeferredContentPresentationTarget.RetainPendingPresentationOnFailure => false;

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        QueueDeferredPresentation();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        RemoveQueuedPresentation();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_presenter is DeferredContentPresenter oldDeferredPresenter)
        {
            oldDeferredPresenter.RemoveQueuedPresentation();
        }

        _presenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        if (_presenter is DeferredContentPresenter deferredPresenter)
        {
            deferredPresenter.RemoveQueuedPresentation();
        }

        _appliedVersion = -1;
        QueueDeferredPresentation();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty || change.Property == ContentTemplateProperty)
        {
            _requestedVersion++;
            QueueDeferredPresentation();
            return;
        }

        if (change.Property == DeferredContentScheduling.TimelineProperty
            || change.Property == DeferredContentScheduling.DelayProperty
            || change.Property == DeferredContentScheduling.OrderProperty)
        {
            QueueDeferredPresentation();
        }
    }

    internal bool ApplyDeferredPresentation()
    {
        return ApplyDeferredPresentation(animateReveal: true);
    }

    internal bool ApplyDeferredPresentation(bool animateReveal)
    {
        if (!IsReadyForPresentation())
        {
            return false;
        }

        object? content = Content;
        IDataTemplate? contentTemplate = ContentTemplate;

        if (_appliedVersion == _requestedVersion
            && ReferenceEquals(_appliedContent, content)
            && ReferenceEquals(_appliedContentTemplate, contentTemplate))
        {
            return true;
        }

        var revealDuration = animateReveal
            ? DeferredContentPresentationTargetHelpers.ResolveRevealDuration(_enqueuedTimeline, this)
            : TimeSpan.Zero;
        ApplyDeferredState(_presenter!, content, contentTemplate, revealDuration);
        _appliedContent = content;
        _appliedContentTemplate = contentTemplate;
        _appliedVersion = _requestedVersion;
        return true;
    }

    bool IDeferredContentPresentationTarget.ApplyDeferredPresentation()
    {
        return ApplyDeferredPresentation();
    }

    private void QueueDeferredPresentation()
    {
        if (!IsReadyForPresentation())
        {
            RemoveQueuedPresentation();
            return;
        }

        if (Content is IDeferredContentPresentation { DeferContentPresentation: false })
        {
            RemoveQueuedPresentation();
            ApplyDeferredPresentation(animateReveal: false);
            return;
        }

        var timeline = DeferredContentPresentationTargetHelpers.ResolveTimeline(this);
        if (!ReferenceEquals(_enqueuedTimeline, timeline))
        {
            RemoveQueuedPresentation();
        }

        timeline.Enqueue(this, DeferredContentPresentationTargetHelpers.ResolveDelay(this), DeferredContentPresentationTargetHelpers.ResolveOrder(this));
    }

    private bool IsReadyForPresentation()
    {
        return _presenter is not null
            && VisualRoot is not null
            && ((ILogical)this).IsAttachedToLogicalTree;
    }

    private void RemoveQueuedPresentation()
    {
        if (_enqueuedTimeline is { } timeline)
        {
            timeline.Remove(this);
        }
    }

    private static void ApplyDeferredState(ContentPresenter presenter, object? content, IDataTemplate? contentTemplate, TimeSpan revealDuration)
    {
        if (presenter is DeferredContentPresenter deferredPresenter)
        {
            deferredPresenter.ApplyDeferredState(content, contentTemplate, revealDuration);
            return;
        }

        var hadPresentedChild = presenter.Child is not null;
        presenter.SetCurrentValue(ContentPresenter.ContentTemplateProperty, contentTemplate);
        presenter.SetCurrentValue(ContentPresenter.ContentProperty, content);
        DeferredContentPresentationTargetHelpers.ApplyRevealAnimation(
            presenter,
            revealDuration,
            hadPresentedChild && content is not null && presenter.Child is not null);
    }
}

internal interface IDeferredContentPresentationTarget
{
    DeferredContentPresentationTimeline? EnqueuedTimeline { get; set; }

    bool RetainPendingPresentationOnFailure { get; }

    bool ApplyDeferredPresentation();
}

/// <summary>
/// A <see cref="ContentPresenter"/> that batches content materialization onto a deferred timeline.
/// </summary>
public class DeferredContentPresenter : ContentPresenter, IDeferredContentPresentationTarget
{
    private bool _suppressDeferredUpdates;
    private object? _requestedContent;
    private IDataTemplate? _requestedContentTemplate;
    private object? _appliedContent;
    private IDataTemplate? _appliedContentTemplate;
    private long _requestedVersion;
    private long _appliedVersion = -1;
    private Size _lastDesiredSize;
    private DeferredContentPresentationTimeline? _enqueuedTimeline;

    DeferredContentPresentationTimeline? IDeferredContentPresentationTarget.EnqueuedTimeline
    {
        get => _enqueuedTimeline;
        set => _enqueuedTimeline = value;
    }

    bool IDeferredContentPresentationTarget.RetainPendingPresentationOnFailure => false;

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        QueueDeferredPresentation();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        RemoveQueuedPresentation();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (ShouldDeferMeasure())
        {
            return _lastDesiredSize;
        }

        var desiredSize = base.MeasureOverride(availableSize);
        _lastDesiredSize = desiredSize;
        return desiredSize;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ContentProperty || change.Property == ContentTemplateProperty)
        {
            if (_suppressDeferredUpdates)
            {
                return;
            }

            if (change.Property == ContentProperty)
            {
                _requestedContent = change.NewValue;
            }
            else
            {
                _requestedContentTemplate = change.NewValue as IDataTemplate;
            }

            _requestedVersion++;
            RestoreAppliedState();
            QueueDeferredPresentation();
            return;
        }

        base.OnPropertyChanged(change);

        if (change.Property == DeferredContentScheduling.TimelineProperty
            || change.Property == DeferredContentScheduling.DelayProperty
            || change.Property == DeferredContentScheduling.OrderProperty)
        {
            QueueDeferredPresentation();
        }
    }

    internal void ApplyDeferredState(object? content, IDataTemplate? contentTemplate)
    {
        ApplyDeferredState(content, contentTemplate, TimeSpan.Zero);
    }

    internal void ApplyDeferredState(object? content, IDataTemplate? contentTemplate, TimeSpan revealDuration)
    {
        var hadPresentedChild = Child is not null;
        _suppressDeferredUpdates = true;

        try
        {
            SetCurrentValue(ContentTemplateProperty, contentTemplate);
            SetCurrentValue(ContentProperty, content);
        }
        finally
        {
            _suppressDeferredUpdates = false;
        }

        UpdatePresentedChild();
        DeferredContentPresentationTargetHelpers.ApplyRevealAnimation(
            this,
            revealDuration,
            hadPresentedChild && content is not null && Child is not null);
    }

    bool IDeferredContentPresentationTarget.ApplyDeferredPresentation()
    {
        return ApplyDeferredPresentation();
    }

    internal bool ApplyDeferredPresentation()
    {
        return ApplyDeferredPresentation(animateReveal: true);
    }

    internal bool ApplyDeferredPresentation(bool animateReveal)
    {
        if (!IsReadyForPresentation())
        {
            return false;
        }

        object? content = _requestedContent;
        IDataTemplate? contentTemplate = _requestedContentTemplate;

        if (_appliedVersion == _requestedVersion
            && ReferenceEquals(_appliedContent, content)
            && ReferenceEquals(_appliedContentTemplate, contentTemplate))
        {
            return true;
        }

        var revealDuration = animateReveal
            ? DeferredContentPresentationTargetHelpers.ResolveRevealDuration(_enqueuedTimeline, this)
            : TimeSpan.Zero;
        ApplyDeferredState(content, contentTemplate, revealDuration);
        _appliedContent = content;
        _appliedContentTemplate = contentTemplate;
        _appliedVersion = _requestedVersion;
        return true;
    }

    internal void RemoveQueuedPresentation()
    {
        if (_enqueuedTimeline is { } timeline)
        {
            timeline.Remove(this);
        }
    }

    private void QueueDeferredPresentation()
    {
        if (TemplatedParent is DeferredContentControl)
        {
            RemoveQueuedPresentation();
            return;
        }

        if (!IsReadyForPresentation())
        {
            RemoveQueuedPresentation();
            return;
        }

        if (_requestedContent is IDeferredContentPresentation { DeferContentPresentation: false })
        {
            RemoveQueuedPresentation();
            ApplyDeferredPresentation(animateReveal: false);
            return;
        }

        var timeline = DeferredContentPresentationTargetHelpers.ResolveTimeline(this);
        if (!ReferenceEquals(_enqueuedTimeline, timeline))
        {
            RemoveQueuedPresentation();
        }

        timeline.Enqueue(this, DeferredContentPresentationTargetHelpers.ResolveDelay(this), DeferredContentPresentationTargetHelpers.ResolveOrder(this));
    }

    private bool IsReadyForPresentation()
    {
        return VisualRoot is not null
            && ((ILogical)this).IsAttachedToLogicalTree;
    }

    private bool ShouldDeferMeasure()
    {
        if (_suppressDeferredUpdates)
        {
            return false;
        }

        if (TemplatedParent is DeferredContentControl)
        {
            return false;
        }

        if (!IsReadyForPresentation())
        {
            return false;
        }

        if (Content is IDeferredContentPresentation { DeferContentPresentation: false })
        {
            return false;
        }

        return _appliedVersion != _requestedVersion;
    }

    private void RestoreAppliedState()
    {
        _suppressDeferredUpdates = true;

        try
        {
            SetCurrentValue(ContentTemplateProperty, _appliedContentTemplate);
            SetCurrentValue(ContentProperty, _appliedContent);
        }
        finally
        {
            _suppressDeferredUpdates = false;
        }

        UpdatePresentedChild();
    }

    private void UpdatePresentedChild()
    {
        UpdateChild();
        PseudoClasses.Set(":empty", Content is null);
        InvalidateMeasure();
    }
}

internal sealed class DeferredContentPresentationTimelineQueue
{
    private static readonly DispatcherPriority s_flushPriority = DispatcherPriority.Background;
    private static readonly Comparison<PendingEntry> s_pendingEntryComparison = ComparePendingEntries;

    private sealed class PendingEntry
    {
        public PendingEntry(IDeferredContentPresentationTarget target, DateTimeOffset dueAt, int order, long sequence)
        {
            Target = target;
            DueAt = dueAt;
            Order = order;
            Sequence = sequence;
        }

        public IDeferredContentPresentationTarget Target { get; }

        public DateTimeOffset DueAt { get; set; }

        public int Order { get; set; }

        public long Sequence { get; set; }
    }

    private readonly DeferredContentPresentationTimeline _timeline;
    private readonly Dictionary<IDeferredContentPresentationTarget, PendingEntry> _pending = new();
    private readonly List<PendingEntry> _dueEntries = new();
    private bool _isScheduled;
    private DateTimeOffset? _scheduledDueAt;
    private long _nextSequence;
    private long _scheduleVersion;

    public DeferredContentPresentationTimelineQueue(DeferredContentPresentationTimeline timeline)
    {
        _timeline = timeline;
    }

    internal bool AutoSchedule { get; set; } = true;

    internal int PendingCount => _pending.Count;

    internal void Enqueue(IDeferredContentPresentationTarget target, TimeSpan delay, int order)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Enqueue(target, delay, order), s_flushPriority);
            return;
        }

        var dueAt = DateTimeOffset.UtcNow + NormalizeDelay(_timeline.InitialDelay + delay);
        var sequence = ++_nextSequence;

        if (_pending.TryGetValue(target, out var entry))
        {
            entry.DueAt = dueAt;
            entry.Order = order;
            entry.Sequence = sequence;
        }
        else
        {
            _pending.Add(target, new PendingEntry(target, dueAt, order, sequence));
        }

        target.EnqueuedTimeline = _timeline;

        if (AutoSchedule)
        {
            ScheduleNextPending(DateTimeOffset.UtcNow);
        }
    }

    internal void Remove(IDeferredContentPresentationTarget target)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Remove(target), s_flushPriority);
            return;
        }

        RemovePending(target);

        if (_pending.Count == 0)
        {
            CancelScheduling();
        }
    }

    internal void FlushPendingBatchForTesting()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(FlushPendingBatchForTesting, s_flushPriority);
            return;
        }

        FlushScheduledBatch(_scheduleVersion);
    }

    private void FlushScheduledBatch(long scheduledVersion)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => FlushScheduledBatch(scheduledVersion), s_flushPriority);
            return;
        }

        if (scheduledVersion != _scheduleVersion)
        {
            return;
        }

        _isScheduled = false;
        _scheduledDueAt = null;

        if (_pending.Count == 0)
        {
            CancelScheduling();
            return;
        }

        var now = DateTimeOffset.UtcNow;
        _dueEntries.Clear();

        foreach (PendingEntry entry in _pending.Values)
        {
            if (entry.DueAt <= now)
            {
                _dueEntries.Add(entry);
            }
        }

        if (_dueEntries.Count == 0)
        {
            if (AutoSchedule)
            {
                ScheduleNextPending(now);
            }

            return;
        }

        _dueEntries.Sort(s_pendingEntryComparison);

        var batchSize = Math.Min(Math.Max(1, _timeline.MaxPresentationsPerPass), _dueEntries.Count);
        var stopwatch = _timeline.BudgetMode == DeferredContentPresentationBudgetMode.RealizationTime
            ? Stopwatch.StartNew()
            : null;
        var completedCount = 0;

        foreach (PendingEntry entry in _dueEntries)
        {
            if (!_pending.TryGetValue(entry.Target, out var current) || !ReferenceEquals(current, entry))
            {
                continue;
            }

            if (entry.Target.ApplyDeferredPresentation())
            {
                RemovePending(entry.Target);
                completedCount++;
            }
            else if (!entry.Target.RetainPendingPresentationOnFailure)
            {
                // Real control targets re-enqueue themselves from attach/template/content change hooks.
                // Dropping not-ready items avoids hot polling loops when a dispatcher drain runs eagerly.
                RemovePending(entry.Target);
            }

            if (!ShouldContinueProcessing(completedCount, batchSize, stopwatch))
            {
                break;
            }
        }

        _dueEntries.Clear();

        if (_pending.Count == 0)
        {
            CancelScheduling();
            return;
        }

        if (!AutoSchedule)
        {
            return;
        }

        now = DateTimeOffset.UtcNow;
        ScheduleFlush(HasDuePending(now) ? _timeline.FollowUpDelay : GetEarliestDueDelay(now));
    }

    private void ScheduleNextPending(DateTimeOffset now)
    {
        if (_pending.Count == 0)
        {
            CancelScheduling();
            return;
        }

        ScheduleFlush(GetEarliestDueDelay(now));
    }

    private void ScheduleFlush(TimeSpan delay)
    {
        var normalizedDelay = NormalizeDelay(delay);
        var dueAt = DateTimeOffset.UtcNow + normalizedDelay;

        if (_isScheduled
            && _scheduledDueAt is { } scheduledDueAt
            && scheduledDueAt <= dueAt)
        {
            return;
        }

        _isScheduled = true;
        _scheduledDueAt = dueAt;

        var scheduledVersion = ++_scheduleVersion;

        if (normalizedDelay == TimeSpan.Zero)
        {
            Dispatcher.UIThread.Post(() => FlushScheduledBatch(scheduledVersion), s_flushPriority);
            return;
        }

        _ = PostDelayedFlushAsync(normalizedDelay, scheduledVersion);
    }

    private void CancelScheduling()
    {
        _isScheduled = false;
        _scheduledDueAt = null;
        _scheduleVersion++;
    }

    private void RemovePending(IDeferredContentPresentationTarget target)
    {
        if (!_pending.Remove(target))
        {
            return;
        }

        if (ReferenceEquals(target.EnqueuedTimeline, _timeline))
        {
            target.EnqueuedTimeline = null;
        }
    }

    private bool ShouldContinueProcessing(int completedCount, int batchSize, Stopwatch? stopwatch)
    {
        return stopwatch is null
            ? completedCount < batchSize
            : stopwatch.Elapsed < _timeline.MaxRealizationTimePerPass;
    }

    private bool HasDuePending(DateTimeOffset now)
    {
        foreach (PendingEntry entry in _pending.Values)
        {
            if (entry.DueAt <= now)
            {
                return true;
            }
        }

        return false;
    }

    private TimeSpan GetEarliestDueDelay(DateTimeOffset now)
    {
        using var enumerator = _pending.Values.GetEnumerator();
        enumerator.MoveNext();

        var earliestDueAt = enumerator.Current.DueAt;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.DueAt < earliestDueAt)
            {
                earliestDueAt = enumerator.Current.DueAt;
            }
        }

        var delay = earliestDueAt - now;
        return NormalizeDelay(delay);
    }

    private static int ComparePendingEntries(PendingEntry left, PendingEntry right)
    {
        var orderComparison = left.Order.CompareTo(right.Order);
        if (orderComparison != 0)
        {
            return orderComparison;
        }

        return left.Sequence.CompareTo(right.Sequence);
    }

    private static TimeSpan NormalizeDelay(TimeSpan delay)
    {
        return delay >= TimeSpan.Zero ? delay : TimeSpan.Zero;
    }

    private async Task PostDelayedFlushAsync(TimeSpan delay, long scheduledVersion)
    {
        await Task.Delay(delay).ConfigureAwait(false);
        Dispatcher.UIThread.Post(() => FlushScheduledBatch(scheduledVersion), s_flushPriority);
    }
}

internal static class DeferredContentPresentationQueue
{
    internal static bool AutoSchedule
    {
        get => DeferredContentPresentationSettings.DefaultTimeline.AutoSchedule;
        set => DeferredContentPresentationSettings.DefaultTimeline.AutoSchedule = value;
    }

    internal static int PendingCount => DeferredContentPresentationSettings.DefaultTimeline.PendingCount;

    internal static void Enqueue(IDeferredContentPresentationTarget target)
    {
        DeferredContentPresentationSettings.DefaultTimeline.Enqueue(target, TimeSpan.Zero, 0);
    }

    internal static void Remove(IDeferredContentPresentationTarget target)
    {
        DeferredContentPresentationSettings.DefaultTimeline.Remove(target);
    }

    internal static void FlushPendingBatchForTesting()
    {
        DeferredContentPresentationSettings.DefaultTimeline.FlushPendingBatchForTesting();
    }
}

internal static class DeferredContentPresentationTargetHelpers
{
    private const double RevealStartingOpacity = 0.85D;
    private static readonly CubicEaseOut s_revealEasing = new();

    private sealed class DeferredRevealTransition : DoubleTransition
    {
    }

    internal static DeferredContentPresentationTimeline ResolveTimeline(AvaloniaObject target)
    {
        if (DeferredContentScheduling.GetTimeline(target) is { } timeline)
        {
            return timeline;
        }

        if (!target.IsSet(DeferredContentScheduling.TimelineProperty)
            && target is StyledElement { TemplatedParent: AvaloniaObject templatedParent })
        {
            return ResolveTimeline(templatedParent);
        }

        return DeferredContentPresentationSettings.DefaultTimeline;
    }

    internal static TimeSpan ResolveDelay(AvaloniaObject target)
    {
        if (!target.IsSet(DeferredContentScheduling.DelayProperty)
            && target is StyledElement { TemplatedParent: AvaloniaObject templatedParent })
        {
            return ResolveDelay(templatedParent);
        }

        var delay = DeferredContentScheduling.GetDelay(target);
        return delay >= TimeSpan.Zero ? delay : TimeSpan.Zero;
    }

    internal static int ResolveOrder(AvaloniaObject target)
    {
        if (!target.IsSet(DeferredContentScheduling.OrderProperty)
            && target is StyledElement { TemplatedParent: AvaloniaObject templatedParent })
        {
            return ResolveOrder(templatedParent);
        }

        return DeferredContentScheduling.GetOrder(target);
    }

    internal static TimeSpan ResolveRevealDuration(DeferredContentPresentationTimeline? activeTimeline, AvaloniaObject target)
    {
        return (activeTimeline ?? ResolveTimeline(target)).RevealDuration;
    }

    internal static void ApplyRevealAnimation(Control control, TimeSpan revealDuration, bool shouldReveal)
    {
        if (!shouldReveal || revealDuration <= TimeSpan.Zero)
        {
            control.Opacity = 1D;
            return;
        }

        EnsureRevealTransition(control, revealDuration);

        control.Opacity = RevealStartingOpacity;
        Dispatcher.UIThread.Post(() => control.SetCurrentValue(Visual.OpacityProperty, 1D), DispatcherPriority.Background);
    }

    private static void EnsureRevealTransition(Control control, TimeSpan revealDuration)
    {
        if (control.Transitions is { } transitions)
        {
            foreach (var transition in transitions)
            {
                if (transition is DeferredRevealTransition revealTransition)
                {
                    revealTransition.Duration = revealDuration;
                    revealTransition.Easing = s_revealEasing;
                    return;
                }

                if (transition is DoubleTransition doubleTransition
                    && doubleTransition.Property == Visual.OpacityProperty)
                {
                    return;
                }
            }

            transitions.Add(CreateRevealTransition(revealDuration));
            return;
        }

        control.Transitions =
        [
            CreateRevealTransition(revealDuration)
        ];
    }

    private static DeferredRevealTransition CreateRevealTransition(TimeSpan revealDuration)
    {
        return new DeferredRevealTransition
        {
            Property = Visual.OpacityProperty,
            Duration = revealDuration,
            Easing = s_revealEasing
        };
    }
}
