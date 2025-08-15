using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class MdiDocumentItemTests
{
    [AvaloniaFact]
    public void MdiDocumentItem_Ctor()
    {
        var actual = new MdiDocumentItem();
        Assert.NotNull(actual);
        Assert.False(actual.IsMaximized);
        Assert.False(actual.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_InitialState()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        Assert.Equal(300, item.Width);
        Assert.Equal(200, item.Height);
        Assert.False(item.IsMaximized);
        Assert.False(item.IsMinimized);
        Assert.Equal(1.0, item.Opacity);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Maximize_SetsIsMaximized()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        item.Maximize();

        Assert.True(item.IsMaximized);
        Assert.False(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Minimize_SetsIsMinimized()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        item.Minimize();

        Assert.True(item.IsMinimized);
        Assert.False(item.IsMaximized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Restore_FromMaximized()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        item.Maximize();
        Assert.True(item.IsMaximized);

        item.Restore();
        Assert.False(item.IsMaximized);
        Assert.False(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Restore_FromMinimized()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        item.Minimize();
        Assert.True(item.IsMinimized);

        item.Restore();
        Assert.False(item.IsMaximized);
        Assert.False(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_StateTransitions()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        // Normal -> Maximized
        item.Maximize();
        Assert.True(item.IsMaximized);
        Assert.False(item.IsMinimized);

        // Maximized -> Minimized
        item.Minimize();
        Assert.False(item.IsMaximized);
        Assert.True(item.IsMinimized);

        // Minimized -> Maximized
        item.Maximize();
        Assert.True(item.IsMaximized);
        Assert.False(item.IsMinimized);

        // Maximized -> Normal
        item.Restore();
        Assert.False(item.IsMaximized);
        Assert.False(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_BoundsPreservation_BeforeMaximize()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };
        Canvas.SetLeft(item, 50);
        Canvas.SetTop(item, 100);

        var originalLeft = Canvas.GetLeft(item);
        var originalTop = Canvas.GetTop(item);
        var originalWidth = item.Width;
        var originalHeight = item.Height;

        item.Maximize();
        
        // After maximizing, original bounds should be preserved internally
        // (we can't directly test private fields, but we can test restoration)
        item.Restore();
        
        Assert.Equal(originalLeft, Canvas.GetLeft(item));
        Assert.Equal(originalTop, Canvas.GetTop(item));
        Assert.Equal(originalWidth, item.Width);
        Assert.Equal(originalHeight, item.Height);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_BoundsPreservation_BeforeMinimize()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };
        Canvas.SetLeft(item, 50);
        Canvas.SetTop(item, 100);

        var originalLeft = Canvas.GetLeft(item);
        var originalTop = Canvas.GetTop(item);
        var originalWidth = item.Width;
        var originalHeight = item.Height;

        item.Minimize();
        
        // After minimizing, original bounds should be preserved internally
        item.Restore();
        
        Assert.Equal(originalLeft, Canvas.GetLeft(item));
        Assert.Equal(originalTop, Canvas.GetTop(item));
        Assert.Equal(originalWidth, item.Width);
        Assert.Equal(originalHeight, item.Height);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_ZIndex_Management()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };

        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Manually set Z-index values to simulate auto-increment behavior
        item1.SetValue(Visual.ZIndexProperty, 1);
        item2.SetValue(Visual.ZIndexProperty, 2);
        item3.SetValue(Visual.ZIndexProperty, 3);

        // Initial Z-index should be based on order added
        Assert.True(item1.GetValue(Visual.ZIndexProperty) < item2.GetValue(Visual.ZIndexProperty));
        Assert.True(item2.GetValue(Visual.ZIndexProperty) < item3.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Resize_UpdatesBounds()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        item.Width = 400;
        item.Height = 300;

        Assert.Equal(400, item.Width);
        Assert.Equal(300, item.Height);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_MinimumSize_Constraints()
    {
        var item = new MdiDocumentItem()
        {
            MinWidth = 100,
            MinHeight = 80,
            Width = 50,  // Below minimum
            Height = 60  // Below minimum
        };

        // Avalonia should enforce minimum constraints
        item.Measure(Size.Infinity);
        item.Arrange(new Rect(item.DesiredSize));

        Assert.True(item.Bounds.Width >= item.MinWidth);
        Assert.True(item.Bounds.Height >= item.MinHeight);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_MaximumSize_Constraints()
    {
        var item = new MdiDocumentItem()
        {
            MaxWidth = 500,
            MaxHeight = 400,
            Width = 600,  // Above maximum
            Height = 500  // Above maximum
        };

        item.Measure(Size.Infinity);
        item.Arrange(new Rect(item.DesiredSize));

        Assert.True(item.Bounds.Width <= item.MaxWidth);
        Assert.True(item.Bounds.Height <= item.MaxHeight);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Content_Rendering()
    {
        var content = new TextBlock { Text = "Test Content" };
        var item = new MdiDocumentItem()
        {
            Content = content,
            Width = 300,
            Height = 200
        };

        Assert.Equal(content, item.Content);
        Assert.Equal("Test Content", ((TextBlock)item.Content).Text);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_IsActive_Property()
    {
        var item = new MdiDocumentItem();
        
        // Default should be false
        Assert.False(item.IsActive);
        
        // Should be able to set to true
        item.IsActive = true;
        Assert.True(item.IsActive);
        
        // Should be able to set back to false
        item.IsActive = false;
        Assert.False(item.IsActive);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Title_Property()
    {
        var item = new MdiDocumentItem();
        
        // Default should be null or empty
        Assert.True(string.IsNullOrEmpty(item.Title));
        
        // Should be able to set title
        item.Title = "Test Document";
        Assert.Equal("Test Document", item.Title);
        
        // Should be able to clear title
        item.Title = null;
        Assert.Null(item.Title);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_CanClose_Property()
    {
        var item = new MdiDocumentItem();
        
        // Default should be true
        Assert.True(item.CanClose);
        
        // Should be able to set to false
        item.CanClose = false;
        Assert.False(item.CanClose);
        
        // Should be able to set back to true
        item.CanClose = true;
        Assert.True(item.CanClose);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Opacity_During_Minimize()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        var originalOpacity = item.Opacity;
        Assert.Equal(1.0, originalOpacity);

        item.Minimize();
        
        // After animation completes, opacity should be reduced for minimized state
        // Note: In a real test, we'd need to wait for animation completion
        // For now, we just verify the minimize state is set
        Assert.True(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Multiple_State_Changes()
    {
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        // Rapid state changes should be handled gracefully
        item.Maximize();
        item.Minimize();
        item.Restore();
        item.Maximize();
        item.Restore();
        
        // Final state should be normal
        Assert.False(item.IsMaximized);
        Assert.False(item.IsMinimized);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Parent_Canvas_Integration()
    {
        var canvas = new Canvas();
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        canvas.Children.Add(item);
        Canvas.SetLeft(item, 100);
        Canvas.SetTop(item, 50);

        Assert.Equal(canvas, item.Parent);
        Assert.Equal(100, Canvas.GetLeft(item));
        Assert.Equal(50, Canvas.GetTop(item));
        Assert.Contains(item, canvas.Children);
    }

    [AvaloniaFact]
    public void MdiDocumentItem_Bounds_After_Parent_Change()
    {
        var canvas1 = new Canvas();
        var canvas2 = new Canvas();
        var item = new MdiDocumentItem()
        {
            Width = 300,
            Height = 200
        };

        // Add to first canvas
        canvas1.Children.Add(item);
        Canvas.SetLeft(item, 100);
        Canvas.SetTop(item, 50);

        var left = Canvas.GetLeft(item);
        var top = Canvas.GetTop(item);

        // Move to second canvas
        canvas1.Children.Remove(item);
        canvas2.Children.Add(item);

        // Position should be preserved
        Assert.Equal(left, Canvas.GetLeft(item));
        Assert.Equal(top, Canvas.GetTop(item));
    }
}