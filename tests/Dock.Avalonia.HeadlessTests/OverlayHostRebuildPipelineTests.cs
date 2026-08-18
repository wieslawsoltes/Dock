using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls.Overlays;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class OverlayHostRebuildPipelineTests
{
    [AvaloniaFact]
    public void RebuildPipeline_Detaches_Content_From_VisualParent()
    {
        var content = new Border();
        var existingPresenter = new ContentPresenter { Content = content };
        var panel = new StackPanel
        {
            Children =
            {
                existingPresenter
            }
        };
        var window = new Window { Content = panel };

        window.Show();
        window.UpdateLayout();

        try
        {
            Assert.Same(existingPresenter, content.GetVisualParent());

            if (content.Parent is not null)
            {
                // Simulate a visual parent without a logical parent.
                ((ISetLogicalParent)content).SetParent(null);
            }

            var host = new OverlayHost { Content = content };
            panel.Children.Add(host);
            window.UpdateLayout();

            Assert.NotNull(content.GetVisualParent());
            Assert.NotSame(existingPresenter, content.GetVisualParent());
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Regression test for Dock #1129: 
    /// </summary>
    [AvaloniaFact]
    public void RebuildPipeline_ReentrantMutationDuringDetach_Is_Coalesced_Not_Recursive()
    {
        var content = new Border();
        var probe = new ReentrancyProbe();

        // `inner` is nested two levels deep (inside a Grid created by the "default" overlay
        // branch, itself nested inside `outer`'s Decorator.Child), so in the natural
        // detach cascade `outer` detaches first and `inner` detaches afterward, as part of
        // the same cascade.
        var inner = new ProbedControl(probe);
        var outer = new ProbedBorder(probe);

        var innerLayer = new OverlayLayer { Overlay = inner, ZIndex = 0 };
        var outerLayer = new OverlayLayer { Overlay = outer, ZIndex = 10 };

        // Set OverlayLayers as an explicit local value so it is not clobbered by the
        // implicit OverlayHost ControlTheme (which sets a default OverlayLayers value
        // containing the built-in Dialog/Confirmation/Busy overlays).
        var layers = new OverlayLayerCollection { innerLayer, outerLayer };

        var panel = new Panel();
        var window = new Window { Content = panel };
        window.Show();
        window.UpdateLayout();

        var host = new OverlayHost
        {
            Content = content,
            UseServiceLayers = false,
            OverlayLayers = layers
        };

        panel.Children.Add(host);
        window.UpdateLayout();

        // Both overlays must be attached from the initial pipeline build.
        Assert.Equal(1, inner.AttachCount);
        Assert.Equal(1, outer.AttachCount);

        // Arm the reentrant mutation: the next time `outer` (which detaches *first* in the
        // natural cascade) is detached, add a brand-new overlay layer directly from inside
        // that detach callback, simulating a popup/service-layer change perturbing the
        // pipeline while it is already being rebuilt.
        var extraOverlay = new Border();
        outer.OnDetached = () => layers.Add(new OverlayLayer { Overlay = extraOverlay, ZIndex = 20 });

        var newContent = new Border();
        var exception = Record.Exception(() =>
        {
            host.Content = newContent;

            // The rebuild triggered by the Content change is coalesced onto the
            // dispatcher; run pending jobs so it actually executes (this is also where
            // the reentrant mutation above fires and is itself coalesced).
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        });

        try
        {
            Assert.Null(exception);
            Assert.Equal(1, probe.MaxObservedDepth);

            // The reentrant mutation itself must still have been applied (as a coalesced
            // follow-up pass), not silently dropped.
            Assert.True(outer.DetachCount >= 1, "outer was never detached; reentrant mutation could not have fired");
            Assert.Equal(3, layers.Count);
            Assert.Contains(layers, layer => ReferenceEquals(layer.Overlay, extraOverlay));

            // The final, settled pipeline must contain the new content and every overlay,
            // each attached with exactly one visual parent - no control left detached
            var descendants = host.GetVisualDescendants().ToList();
            Assert.Contains(newContent, descendants);
            Assert.Contains(inner, descendants);
            Assert.Contains(outer, descendants);
            Assert.Contains(extraOverlay, descendants);

            Assert.NotNull(newContent.GetVisualParent());
            Assert.NotNull(inner.GetVisualParent());
            Assert.NotNull(outer.GetVisualParent());
            Assert.NotNull(extraOverlay.GetVisualParent());
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Regression test for the reported #1129 symptom
    /// </summary>
    [AvaloniaFact]
    public void OverlayLayersChanged_StructurallyEquivalentReplacement_Is_Noop()
    {
        var content = new Border();
        var overlay1 = new Border();
        var layers1 = new OverlayLayerCollection { new OverlayLayer { Overlay = overlay1, ZIndex = 5 } };

        var panel = new Panel();
        var window = new Window { Content = panel };
        window.Show();
        window.UpdateLayout();

        var host = new OverlayHost
        {
            Content = content,
            UseServiceLayers = false,
            OverlayLayers = layers1
        };

        panel.Children.Add(host);
        window.UpdateLayout();

        var childBefore = host.Child;
        Assert.NotNull(childBefore);
        Assert.NotNull(overlay1.GetVisualParent());

        // A brand-new collection with a brand-new overlay control of the same
        // type/z-index - structurally equivalent to what's already hosted, exactly like
        // a fresh DynamicResource resolution of a shared="false" overlay-layers resource.
        var overlay2 = new Border();
        var layers2 = new OverlayLayerCollection { new OverlayLayer { Overlay = overlay2, ZIndex = 5 } };

        host.OverlayLayers = layers2;

        // Drain the dispatcher so that a rebuild would actually execute if one had been
        // (incorrectly) requested - without this, a coalesced-but-still-pending rebuild
        // would make this assertion pass for the wrong reason.
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        // The pipeline must not have been rebuilt: the same Child instance, the original
        // overlay control still attached and untouched, and the "new" replacement
        // overlay control never used/attached.
        Assert.Same(childBefore, host.Child);
        Assert.NotNull(overlay1.GetVisualParent());
        Assert.Null(overlay2.GetVisualParent());
        Assert.Contains(content, host.GetVisualDescendants());

        window.Close();
    }

    /// <summary>
    /// A genuinely different <see cref="OverlayHost.OverlayLayers"/> replacement (not
    /// just a structurally-equivalent re-resolution) must still trigger a real rebuild,
    /// proving the no-op short-circuit above does not suppress legitimate changes.
    /// </summary>
    [AvaloniaFact]
    public void OverlayLayersChanged_GenuinelyDifferentReplacement_Rebuilds()
    {
        var content = new Border();
        var overlay1 = new Border();
        var layer1 = new OverlayLayer { Overlay = overlay1, ZIndex = 5 };
        var layers1 = new OverlayLayerCollection { layer1 };

        var panel = new Panel();
        var window = new Window { Content = panel };
        window.Show();
        window.UpdateLayout();

        var host = new OverlayHost
        {
            Content = content,
            UseServiceLayers = false,
            OverlayLayers = layers1
        };

        panel.Children.Add(host);
        window.UpdateLayout();

        // A genuinely different layer set (an extra layer) must still trigger a rebuild.
        var overlay2 = new Border();
        var layers2 = new OverlayLayerCollection { layer1, new OverlayLayer { Overlay = overlay2, ZIndex = 10 } };

        host.OverlayLayers = layers2;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.NotNull(overlay2.GetVisualParent());
        Assert.Contains(content, host.GetVisualDescendants());

        window.Close();
    }
    /// records how many of their attach/detach callbacks were simultaneously on the call
    /// stack, so a test can detect nested (recursive) pipeline rebuild execution.
    /// </summary>
    private sealed class ReentrancyProbe
    {
        public int ActiveDepth;

        public int MaxObservedDepth;

        public void Enter()
        {
            ActiveDepth++;
            MaxObservedDepth = System.Math.Max(MaxObservedDepth, ActiveDepth);
        }

        public void Exit() => ActiveDepth--;
    }

    /// <summary>
    /// A plain overlay control (hits the "default" Grid-wrapping branch in
    /// <c>OverlayHost.RebuildPipeline</c>) that records attach/detach counts against a
    /// shared <see cref="ReentrancyProbe"/>.
    /// </summary>
    private sealed class ProbedControl : Control
    {
        private readonly ReentrancyProbe _probe;

        public ProbedControl(ReentrancyProbe probe) => _probe = probe;

        public int AttachCount { get; private set; }

        public int DetachCount { get; private set; }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AttachCount++;
            _probe.Enter();
            _probe.Exit();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            DetachCount++;
            _probe.Enter();
            _probe.Exit();
        }
    }

    /// <summary>
    /// An overlay control (hits the "Decorator" branch in
    /// <c>OverlayHost.RebuildPipeline</c>, so it wraps directly around the rest of the
    /// pipeline and detaches first in the natural cascade) that can invoke a
    /// caller-supplied callback from inside its own detach callback, used to simulate a
    /// popup/service-layer change reentrantly mutating the pipeline while a rebuild is
    /// tearing down the previous one.
    /// </summary>
    private sealed class ProbedBorder : Border
    {
        private readonly ReentrancyProbe _probe;

        public ProbedBorder(ReentrancyProbe probe) => _probe = probe;

        public int AttachCount { get; private set; }

        public int DetachCount { get; private set; }

        public System.Action? OnDetached { get; set; }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AttachCount++;
            _probe.Enter();
            _probe.Exit();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            DetachCount++;
            _probe.Enter();

            try
            {
                var callback = OnDetached;
                OnDetached = null;
                callback?.Invoke();
            }
            finally
            {
                _probe.Exit();
            }
        }
    }
}

