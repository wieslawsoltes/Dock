using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTargetPresentationTests
{
    [AvaloniaTheory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Bottom)]
    [InlineData(DockOperation.Fill)]
    [InlineData(DockOperation.OuterLeft)]
    [InlineData(DockOperation.OuterTop)]
    [InlineData(DockOperation.OuterRight)]
    [InlineData(DockOperation.OuterBottom)]
    public void External_Selectors_Use_Native_Coordinates_And_Validation(DockOperation operation)
    {
        using Workspace workspace = new(operation);
        int validations = 0;
        Point pointer = workspace.Pointer;
        Assert.Same(workspace.Selector, workspace.Target.GetSelector(operation));
        Assert.Equal(operation, workspace.Target.GetDockOperation(pointer, workspace.Surface, workspace.Surface,
            DragAction.Move, (point, candidate, action, relative) =>
            {
                validations++;
                Assert.Equal(pointer, point);
                Assert.Equal(operation, candidate);
                Assert.Equal(DragAction.Move, action);
                Assert.Same(workspace.Surface, relative);
                return true;
            }));
        Assert.Equal(1, validations);
        Assert.Equal((true, true), workspace.Presentation.States[operation]);
        Assert.Single(workspace.Presentation.States, pair => pair.Value.Active);

        Assert.Equal(DockOperation.Window, workspace.Hover(validate: false));
        Assert.Equal((true, false), workspace.Presentation.States[operation]);
        Assert.Equal(DockOperation.Window, workspace.Hover(visible: false));
        Assert.Equal((false, false), workspace.Presentation.States[operation]);
        Assert.Equal(operation, workspace.Hover());
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left, true)]
    [InlineData(DockOperation.OuterRight, true)]
    [InlineData(DockOperation.Top, false)]
    [InlineData(DockOperation.OuterBottom, false)]
    public void Direction_Limits_And_Indicator_Only_Mode_Still_Apply(DockOperation operation, bool horizontal)
    {
        using Workspace workspace = new(operation);
        DockProperties.SetIndicatorDockOperation(workspace.Surface, operation);
        workspace.Target.ShowIndicatorsOnly = true;
        Assert.Equal(operation, workspace.Hover(validate: false));
        Assert.Equal((false, true), workspace.Presentation.States[operation]);
        if (horizontal) workspace.Target.ShowHorizontalTargets = false;
        else workspace.Target.ShowVerticalTargets = false;
        Assert.Equal(DockOperation.Window, workspace.Hover());
        Assert.Equal((false, false), workspace.Presentation.States[operation]);
        ((IDockTarget)workspace.Target).Reset();
        Assert.True(workspace.Target.ShowHorizontalTargets);
        Assert.True(workspace.Target.ShowVerticalTargets);
        Assert.All(workspace.Presentation.States.Values, state => Assert.Equal((false, false), state));
        Assert.Equal(operation, workspace.Hover());
    }

    [AvaloniaFact]
    public void Replacement_And_Reentrant_Validation_Clear_The_Previous_Presentation()
    {
        using Workspace workspace = new(DockOperation.Fill);
        Assert.Equal(DockOperation.Fill, workspace.Hover());
        Presentation replacement = new(DockOperation.Fill, workspace.Selector);
        Assert.Equal(DockOperation.Window, workspace.Target.GetDockOperation(workspace.Pointer,
            workspace.Surface, workspace.Surface, DragAction.Move, (_, _, _, _) =>
            {
                workspace.Target.Presentation = replacement;
                return true;
            }));
        Assert.All(workspace.Presentation.States.Values, state => Assert.Equal((false, false), state));
        Assert.Empty(replacement.States);
        Assert.Equal(DockOperation.Fill, workspace.Hover());
        workspace.Target.Presentation = null;
        Assert.All(replacement.States.Values, state => Assert.Equal((false, false), state));
        Assert.NotSame(workspace.Selector, workspace.Target.GetSelector(DockOperation.Fill));
    }

    [AvaloniaFact]
    public void Policy_Opacity_Preserves_Hit_Testing_But_Explicitly_Hidden_Selectors_Are_Disabled()
    {
        using Workspace workspace = new(DockOperation.Fill);
        workspace.Selector.Opacity = 0;
        Assert.Equal(DockOperation.Fill, workspace.Hover());
        workspace.Selector.IsVisible = false;
        Assert.Equal(DockOperation.Window, workspace.Hover());
        Assert.Equal((false, false), workspace.Presentation.States[DockOperation.Fill]);
        workspace.Selector.IsVisible = true;
        workspace.Overlay.UpdateLayout();
        Assert.Equal(DockOperation.Fill, workspace.Hover());
    }

    [AvaloniaFact]
    public void Reentrant_Visibility_Cannot_Reactivate_A_Removed_Presentation()
    {
        using Workspace workspace = new(DockOperation.Fill);
        Assert.Equal(DockOperation.Fill, workspace.Hover());
        Assert.Equal(DockOperation.Window, workspace.Target.GetDockOperation(workspace.Pointer,
            workspace.Surface, workspace.Surface, DragAction.Move, (_, _, _, _) => throw new InvalidOperationException(),
            (_, _, _, _) =>
            {
                workspace.Target.Presentation = null;
                return true;
            }));
        Assert.All(workspace.Presentation.States.Values, state => Assert.Equal((false, false), state));
    }

    [AvaloniaFact]
    public void Global_Presentation_Retains_Native_Edge_Thickness_And_Default_Operation()
    {
        using Workspace workspace = new(DockOperation.Left, global: true);
        GlobalDockTarget target = (GlobalDockTarget)workspace.Target;
        Assert.Equal(DockOperation.Left, workspace.Hover());
        target.EdgeHitTestThickness = 1;
        Assert.Equal(DockOperation.None, workspace.Hover());
        Assert.Equal((true, false), workspace.Presentation.States[DockOperation.Left]);
        target.EdgeHitTestThickness = double.NaN;
        Assert.Equal(DockOperation.Left, workspace.Hover());
    }

    [AvaloniaFact]
    public void Detached_Or_Omitted_Selectors_Cannot_Activate_An_Operation()
    {
        using Workspace workspace = new(DockOperation.Fill);
        Point pointer = workspace.Pointer;
        workspace.Overlay.Content = null;
        Assert.Equal(DockOperation.Window, workspace.Target.GetDockOperation(pointer, workspace.Surface,
            workspace.Surface, DragAction.Move, (_, _, _, _) => true));
        workspace.Presentation.Selector = null;
        Assert.Null(workspace.Target.GetSelector(DockOperation.Fill));
        Assert.Equal(DockOperation.Window, workspace.Hover());
        Assert.All(workspace.Presentation.States.Values, state => Assert.Equal((false, false), state));
    }

    private sealed class Presentation(DockOperation operation, Control selector) : IDockTargetPresentation
    {
        internal Control? Selector { get; set; } = selector;
        internal Dictionary<DockOperation, (bool Visible, bool Active)> States { get; } = new();
        public Control? GetSelector(DockOperation candidate) => candidate == operation ? Selector : null;
        public void SetOperationState(DockOperation candidate, bool visible, bool active) => States[candidate] = (visible, active);
    }

    private sealed class Workspace : IDisposable
    {
        internal Border Surface { get; } = new();
        internal Border Selector { get; } = new() { Width = 60, Height = 40,
            RenderTransform = new ScaleTransform(1.5, 1.5) };
        internal DockTargetBase Target { get; }
        internal Presentation Presentation { get; }
        internal Window Owner { get; }
        internal Window Overlay { get; }
        internal Point Pointer => TopLevel.GetTopLevel(Selector) is not null
            ? Surface.PointToClient(Selector.PointToScreen(new Point(30, 20))) : default;

        internal Workspace(DockOperation operation, bool global = false)
        {
            Target = global ? new GlobalDockTarget() : new DockTarget();
            Presentation = new(operation, Selector);
            Target.Presentation = Presentation;
            Owner = new Window { Width = 600, Height = 400, Content = new Panel { Children = { Surface, Target } } };
            Overlay = new Window { Width = 160, Height = 120, Content = Selector };
            Owner.Show();
            Overlay.Show(Owner);
            Owner.UpdateLayout(); Overlay.UpdateLayout();
        }

        internal DockOperation Hover(bool validate = true, bool visible = true) => Target.GetDockOperation(Pointer,
            Surface, Surface, DragAction.Move, (_, _, _, _) => validate, (_, _, _, _) => visible);
        public void Dispose() { Target.Presentation = null; Overlay.Close(); Owner.Close(); }
    }
}
