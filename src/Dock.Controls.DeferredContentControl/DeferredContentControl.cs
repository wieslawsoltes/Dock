// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
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

    internal void ApplyDeferredPresentation()
    {
        if (!IsReadyForPresentation())
        {
            return;
        }

        object? content = Content;
        IDataTemplate? contentTemplate = ContentTemplate;

        if (_appliedVersion == _requestedVersion
            && ReferenceEquals(_appliedContent, content)
            && ReferenceEquals(_appliedContentTemplate, contentTemplate))
        {
            return;
        }

        ApplyDeferredState(_presenter!, content, contentTemplate);
        _appliedContent = content;
        _appliedContentTemplate = contentTemplate;
        _appliedVersion = _requestedVersion;
    }

    void IDeferredContentPresentationTarget.ApplyDeferredPresentation()
    {
        ApplyDeferredPresentation();
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
    void ApplyDeferredPresentation();
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

    void IDeferredContentPresentationTarget.ApplyDeferredPresentation()
    {
        ApplyDeferredPresentation();
    }

    internal void ApplyDeferredPresentation()
    {
        if (!IsReadyForPresentation())
        {
            return;
        }

        object? content = Content;
        IDataTemplate? contentTemplate = ContentTemplate;

        if (_appliedVersion == _requestedVersion
            && ReferenceEquals(_appliedContent, content)
            && ReferenceEquals(_appliedContentTemplate, contentTemplate))
        {
            return;
        }

        ApplyDeferredState(content, contentTemplate);
        _appliedContent = content;
        _appliedContentTemplate = contentTemplate;
        _appliedVersion = _requestedVersion;
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
    private static readonly HashSet<IDeferredContentPresentationTarget> s_pending = new();
    private static bool s_isScheduled;

    internal static void Enqueue(IDeferredContentPresentationTarget control)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Enqueue(control), DispatcherPriority.Render);
            return;
        }

        if (!s_pending.Add(control))
        {
            return;
        }

        ScheduleFlush();
    }

    internal static void Remove(IDeferredContentPresentationTarget control)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Remove(control), DispatcherPriority.Render);
            return;
        }

        s_pending.Remove(control);
    }

    private static void ScheduleFlush()
    {
        if (s_isScheduled)
        {
            return;
        }

        s_isScheduled = true;
        Dispatcher.UIThread.Post(Flush, DispatcherPriority.Render);
    }

    private static void Flush()
    {
        s_isScheduled = false;

        if (s_pending.Count == 0)
        {
            return;
        }

        var batch = new IDeferredContentPresentationTarget[s_pending.Count];
        s_pending.CopyTo(batch);
        s_pending.Clear();

        foreach (IDeferredContentPresentationTarget control in batch)
        {
            control.ApplyDeferredPresentation();
        }

        if (s_pending.Count > 0)
        {
            ScheduleFlush();
        }
    }
}
