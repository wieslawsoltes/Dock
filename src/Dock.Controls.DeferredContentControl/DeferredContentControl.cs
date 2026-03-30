// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Threading;

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
/// A <see cref="ContentControl"/> that batches content materialization onto the next dispatcher pass.
/// </summary>
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class DeferredContentControl : ContentControl, IDeferredContentPresentationTarget
{
    private ContentPresenter? _presenter;
    private object? _appliedContent;
    private IDataTemplate? _appliedContentTemplate;
    private long _requestedVersion;
    private long _appliedVersion = -1;

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
        DeferredContentPresentationQueue.Remove(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        if (_presenter is IDeferredContentPresentationTarget deferredPresenter)
        {
            DeferredContentPresentationQueue.Remove(deferredPresenter);
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
        }
    }

    internal bool ApplyDeferredPresentation()
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

        ApplyDeferredState(_presenter!, content, contentTemplate);
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
            return;
        }

        if (Content is IDeferredContentPresentation { DeferContentPresentation: false })
        {
            DeferredContentPresentationQueue.Remove(this);
            ApplyDeferredPresentation();
            return;
        }

        DeferredContentPresentationQueue.Enqueue(this);
    }

    private bool IsReadyForPresentation()
    {
        return _presenter is not null
            && VisualRoot is not null
            && ((ILogical)this).IsAttachedToLogicalTree;
    }

    private static void ApplyDeferredState(ContentPresenter presenter, object? content, IDataTemplate? contentTemplate)
    {
        if (presenter is DeferredContentPresenter deferredPresenter)
        {
            deferredPresenter.ApplyDeferredState(content, contentTemplate);
            return;
        }

        presenter.SetCurrentValue(ContentPresenter.ContentTemplateProperty, contentTemplate);
        presenter.SetCurrentValue(ContentPresenter.ContentProperty, content);
    }
}

internal interface IDeferredContentPresentationTarget
{
    bool ApplyDeferredPresentation();
}

/// <summary>
/// A <see cref="ContentPresenter"/> that batches content materialization onto the next dispatcher pass.
/// </summary>
public class DeferredContentPresenter : ContentPresenter, IDeferredContentPresentationTarget
{
    private bool _suppressDeferredUpdates;
    private object? _appliedContent;
    private IDataTemplate? _appliedContentTemplate;
    private long _requestedVersion;
    private long _appliedVersion = -1;

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
        DeferredContentPresentationQueue.Remove(this);
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

            _requestedVersion++;
            QueueDeferredPresentation();
            return;
        }

        base.OnPropertyChanged(change);
    }

    internal void ApplyDeferredState(object? content, IDataTemplate? contentTemplate)
    {
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
    }

    bool IDeferredContentPresentationTarget.ApplyDeferredPresentation()
    {
        return ApplyDeferredPresentation();
    }

    internal bool ApplyDeferredPresentation()
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

        ApplyDeferredState(content, contentTemplate);
        _appliedContent = content;
        _appliedContentTemplate = contentTemplate;
        _appliedVersion = _requestedVersion;
        return true;
    }

    private void QueueDeferredPresentation()
    {
        if (TemplatedParent is DeferredContentControl)
        {
            DeferredContentPresentationQueue.Remove(this);
            return;
        }

        if (!IsReadyForPresentation())
        {
            return;
        }

        if (Content is IDeferredContentPresentation { DeferContentPresentation: false })
        {
            DeferredContentPresentationQueue.Remove(this);
            ApplyDeferredPresentation();
            return;
        }

        DeferredContentPresentationQueue.Enqueue(this);
    }

    private bool IsReadyForPresentation()
    {
        return VisualRoot is not null
            && ((ILogical)this).IsAttachedToLogicalTree;
    }

    private void UpdatePresentedChild()
    {
        UpdateChild();
        PseudoClasses.Set(":empty", Content is null);
        InvalidateMeasure();
    }
}

internal static class DeferredContentPresentationQueue
{
    internal static bool AutoSchedule { get; set; } = true;
    internal static int MaxPresentationsPerFrame { get; set; } = 8;
    internal static int PendingCount => s_pendingLookup.Count;

    private static readonly LinkedList<IDeferredContentPresentationTarget> s_pending = new();
    private static readonly Dictionary<IDeferredContentPresentationTarget, LinkedListNode<IDeferredContentPresentationTarget>> s_pendingLookup = new();
    private static readonly TimeSpan s_followUpFlushDelay = TimeSpan.FromMilliseconds(1);
    private static bool s_isScheduled;

    internal static void Enqueue(IDeferredContentPresentationTarget control)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Enqueue(control), DispatcherPriority.Render);
            return;
        }

        if (s_pendingLookup.ContainsKey(control))
        {
            return;
        }

        var node = s_pending.AddLast(control);
        s_pendingLookup.Add(control, node);

        if (AutoSchedule)
        {
            ScheduleFlush(delayed: false);
        }
    }

    internal static void Remove(IDeferredContentPresentationTarget control)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Remove(control), DispatcherPriority.Render);
            return;
        }

        RemovePending(control);

        if (s_pendingLookup.Count == 0)
        {
            CancelScheduling();
        }
    }

    private static void ScheduleFlush(bool delayed)
    {
        if (s_isScheduled)
        {
            return;
        }

        s_isScheduled = true;

        if (delayed)
        {
            DispatcherTimer.RunOnce(FlushScheduledBatch, s_followUpFlushDelay, DispatcherPriority.Render);
            return;
        }

        Dispatcher.UIThread.Post(FlushScheduledBatch, DispatcherPriority.Render);
    }

    internal static void FlushPendingBatchForTesting()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(FlushPendingBatchForTesting, DispatcherPriority.Render);
            return;
        }

        FlushScheduledBatch();
    }

    private static void FlushScheduledBatch()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(FlushScheduledBatch, DispatcherPriority.Render);
            return;
        }

        if (s_pendingLookup.Count == 0)
        {
            CancelScheduling();
            return;
        }

        s_isScheduled = false;

        var batchSize = Math.Min(Math.Max(1, MaxPresentationsPerFrame), s_pendingLookup.Count);
        var node = s_pending.First;
        var completedCount = 0;

        while (node is not null && completedCount < batchSize)
        {
            var current = node;
            node = node.Next;

            if (current.Value.ApplyDeferredPresentation())
            {
                RemovePending(current.Value);
                completedCount++;
            }
        }

        if (s_pendingLookup.Count == 0)
        {
            CancelScheduling();
            return;
        }

        if (AutoSchedule)
        {
            ScheduleFlush(delayed: true);
        }
    }

    private static void CancelScheduling()
    {
        if (!s_isScheduled)
        {
            return;
        }

        s_isScheduled = false;
    }

    private static void RemovePending(IDeferredContentPresentationTarget control)
    {
        if (!s_pendingLookup.Remove(control, out var node))
        {
            return;
        }

        s_pending.Remove(node);
    }
}
