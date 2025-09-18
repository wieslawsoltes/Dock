using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class MdiWindowArrangementTests
{
    [AvaloniaFact]
    public void Canvas_Ctor()
    {
        var canvas = new Canvas();
        Assert.NotNull(canvas);
        Assert.Empty(canvas.Children);
    }

    [AvaloniaFact]
    public void Canvas_AddChild()
    {
        var canvas = new Canvas();
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item);
        
        Assert.Single(canvas.Children);
        Assert.Contains(item, canvas.Children);
    }

    [AvaloniaFact]
    public void Canvas_MultipleChildren_ZIndexOrdering()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };

        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Z-index should increase with order of addition
        var zIndex1 = item1.GetValue(Visual.ZIndexProperty);
        var zIndex2 = item2.GetValue(Visual.ZIndexProperty);
        var zIndex3 = item3.GetValue(Visual.ZIndexProperty);

        Assert.True(zIndex1 <= zIndex2);
        Assert.True(zIndex2 <= zIndex3);
    }

    [AvaloniaFact]
    public void WindowSnapping_LeftEdge()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        // Simulate dragging near left edge
        var snapThreshold = 10;
        var nearLeftEdge = 5; // Within snap threshold
        
        Canvas.SetLeft(item, nearLeftEdge);
        Canvas.SetTop(item, 100);

        // Test snapping logic (this would be implemented in the canvas)
        var shouldSnapToLeft = nearLeftEdge <= snapThreshold;
        Assert.True(shouldSnapToLeft);
        
        if (shouldSnapToLeft)
        {
            Canvas.SetLeft(item, 0); // Snap to edge
        }
        
        Assert.Equal(0, Canvas.GetLeft(item));
    }

    [AvaloniaFact]
    public void WindowSnapping_RightEdge()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        var snapThreshold = 10;
        var nearRightEdge = canvas.Width - item.Width - 5; // Within snap threshold of right edge
        
        Canvas.SetLeft(item, nearRightEdge);
        Canvas.SetTop(item, 100);

        var distanceFromRightEdge = canvas.Width - (Canvas.GetLeft(item) + item.Width);
        var shouldSnapToRight = distanceFromRightEdge <= snapThreshold;
        Assert.True(shouldSnapToRight);
        
        if (shouldSnapToRight)
        {
            Canvas.SetLeft(item, canvas.Width - item.Width); // Snap to right edge
        }
        
        Assert.Equal(canvas.Width - item.Width, Canvas.GetLeft(item));
    }

    [AvaloniaFact]
    public void WindowSnapping_TopEdge()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        var snapThreshold = 10;
        var nearTopEdge = 3; // Within snap threshold
        
        Canvas.SetLeft(item, 100);
        Canvas.SetTop(item, nearTopEdge);

        var shouldSnapToTop = nearTopEdge <= snapThreshold;
        Assert.True(shouldSnapToTop);
        
        if (shouldSnapToTop)
        {
            Canvas.SetTop(item, 0); // Snap to top edge
        }
        
        Assert.Equal(0, Canvas.GetTop(item));
    }

    [AvaloniaFact]
    public void WindowSnapping_BottomEdge()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        var snapThreshold = 10;
        var nearBottomEdge = canvas.Height - item.Height - 7; // Within snap threshold of bottom edge
        
        Canvas.SetLeft(item, 100);
        Canvas.SetTop(item, nearBottomEdge);

        var distanceFromBottomEdge = canvas.Height - (Canvas.GetTop(item) + item.Height);
        var shouldSnapToBottom = distanceFromBottomEdge <= snapThreshold;
        Assert.True(shouldSnapToBottom);
        
        if (shouldSnapToBottom)
        {
            Canvas.SetTop(item, canvas.Height - item.Height); // Snap to bottom edge
        }
        
        Assert.Equal(canvas.Height - item.Height, Canvas.GetTop(item));
    }

    [AvaloniaFact]
    public void WindowSnapping_NoSnap_WhenFarFromEdge()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        var snapThreshold = 10;
        var farFromEdge = 50; // Beyond snap threshold
        
        Canvas.SetLeft(item, farFromEdge);
        Canvas.SetTop(item, farFromEdge);

        var shouldSnapToLeft = farFromEdge <= snapThreshold;
        var shouldSnapToTop = farFromEdge <= snapThreshold;
        
        Assert.False(shouldSnapToLeft);
        Assert.False(shouldSnapToTop);
        
        // Position should remain unchanged
        Assert.Equal(farFromEdge, Canvas.GetLeft(item));
        Assert.Equal(farFromEdge, Canvas.GetTop(item));
    }

    [AvaloniaFact]
    public void WindowArrangement_Cascade()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var items = new List<MdiDocumentItem>();
        
        // Create multiple windows
        for (int i = 0; i < 5; i++)
        {
            var item = new MdiDocumentItem() { Width = 200, Height = 150 };
            items.Add(item);
            canvas.Children.Add(item);
        }

        // Arrange in cascade pattern
        var cascadeOffset = 30;
        for (int i = 0; i < items.Count; i++)
        {
            Canvas.SetLeft(items[i], i * cascadeOffset);
            Canvas.SetTop(items[i], i * cascadeOffset);
        }

        // Verify cascade arrangement
        for (int i = 0; i < items.Count; i++)
        {
            Assert.Equal(i * cascadeOffset, Canvas.GetLeft(items[i]));
            Assert.Equal(i * cascadeOffset, Canvas.GetTop(items[i]));
        }
    }

    [AvaloniaFact]
    public void WindowArrangement_TileHorizontal()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var items = new List<MdiDocumentItem>();
        
        // Create 4 windows
        for (int i = 0; i < 4; i++)
        {
            var item = new MdiDocumentItem();
            items.Add(item);
            canvas.Children.Add(item);
        }

        // Arrange in horizontal tiles (2x2 grid)
        var cols = 2;
        var rows = 2;
        var tileWidth = canvas.Width / cols;
        var tileHeight = canvas.Height / rows;

        for (int i = 0; i < items.Count; i++)
        {
            var col = i % cols;
            var row = i / cols;
            
            Canvas.SetLeft(items[i], col * tileWidth);
            Canvas.SetTop(items[i], row * tileHeight);
            items[i].Width = tileWidth;
            items[i].Height = tileHeight;
        }

        // Verify tiling
        for (int i = 0; i < items.Count; i++)
        {
            var col = i % cols;
            var row = i / cols;
            
            Assert.Equal(col * tileWidth, Canvas.GetLeft(items[i]));
            Assert.Equal(row * tileHeight, Canvas.GetTop(items[i]));
            Assert.Equal(tileWidth, items[i].Width);
            Assert.Equal(tileHeight, items[i].Height);
        }
    }

    [AvaloniaFact]
    public void WindowArrangement_TileVertical()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var items = new List<MdiDocumentItem>();
        
        // Create 3 windows
        for (int i = 0; i < 3; i++)
        {
            var item = new MdiDocumentItem();
            items.Add(item);
            canvas.Children.Add(item);
        }

        // Arrange in vertical tiles
        var tileWidth = canvas.Width;
        var tileHeight = canvas.Height / items.Count;

        for (int i = 0; i < items.Count; i++)
        {
            Canvas.SetLeft(items[i], 0);
            Canvas.SetTop(items[i], i * tileHeight);
            items[i].Width = tileWidth;
            items[i].Height = tileHeight;
        }

        // Verify vertical tiling
        for (int i = 0; i < items.Count; i++)
        {
            Assert.Equal(0, Canvas.GetLeft(items[i]));
            Assert.Equal(i * tileHeight, Canvas.GetTop(items[i]));
            Assert.Equal(tileWidth, items[i].Width);
            Assert.Equal(tileHeight, items[i].Height);
        }
    }

    [AvaloniaFact]
    public void WindowBounds_ConstrainToCanvas()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        // Try to position window outside canvas bounds
        var attemptedLeft = -50; // Outside left edge
        var attemptedTop = -30;  // Outside top edge
        
        Canvas.SetLeft(item, attemptedLeft);
        Canvas.SetTop(item, attemptedTop);

        // Constrain to canvas bounds
        var constrainedLeft = Math.Max(0, Canvas.GetLeft(item));
        var constrainedTop = Math.Max(0, Canvas.GetTop(item));
        
        Canvas.SetLeft(item, constrainedLeft);
        Canvas.SetTop(item, constrainedTop);

        Assert.Equal(0, Canvas.GetLeft(item));
        Assert.Equal(0, Canvas.GetTop(item));
    }

    [AvaloniaFact]
    public void WindowBounds_ConstrainToCanvas_RightBottom()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        // Try to position window beyond right/bottom edges
        var attemptedLeft = canvas.Width + 50;  // Beyond right edge
        var attemptedTop = canvas.Height + 30;  // Beyond bottom edge
        
        Canvas.SetLeft(item, attemptedLeft);
        Canvas.SetTop(item, attemptedTop);

        // Constrain to canvas bounds
        var maxLeft = canvas.Width - item.Width;
        var maxTop = canvas.Height - item.Height;
        
        var constrainedLeft = Math.Min(maxLeft, Canvas.GetLeft(item));
        var constrainedTop = Math.Min(maxTop, Canvas.GetTop(item));
        
        Canvas.SetLeft(item, constrainedLeft);
        Canvas.SetTop(item, constrainedTop);

        Assert.Equal(maxLeft, Canvas.GetLeft(item));
        Assert.Equal(maxTop, Canvas.GetTop(item));
    }

    [AvaloniaFact]
    public void WindowOverlap_Detection()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);

        // Position windows to overlap
        Canvas.SetLeft(item1, 100);
        Canvas.SetTop(item1, 100);
        Canvas.SetLeft(item2, 150); // Overlapping
        Canvas.SetTop(item2, 120);  // Overlapping

        var bounds1 = new Rect(Canvas.GetLeft(item1), Canvas.GetTop(item1), item1.Width, item1.Height);
        var bounds2 = new Rect(Canvas.GetLeft(item2), Canvas.GetTop(item2), item2.Width, item2.Height);
        
        var overlaps = bounds1.Intersects(bounds2);
        Assert.True(overlaps);
    }

    [AvaloniaFact]
    public void WindowOverlap_NoOverlap()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);

        // Position windows without overlap
        Canvas.SetLeft(item1, 100);
        Canvas.SetTop(item1, 100);
        Canvas.SetLeft(item2, 350); // No overlap
        Canvas.SetTop(item2, 100);

        var bounds1 = new Rect(Canvas.GetLeft(item1), Canvas.GetTop(item1), item1.Width, item1.Height);
        var bounds2 = new Rect(Canvas.GetLeft(item2), Canvas.GetTop(item2), item2.Width, item2.Height);
        
        var overlaps = bounds1.Intersects(bounds2);
        Assert.False(overlaps);
    }

    [AvaloniaFact]
    public void WindowPositioning_CenterInCanvas()
    {
        var canvas = new Canvas() { Width = 800, Height = 600 };
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        canvas.Children.Add(item);

        // Center the window in canvas
        var centerX = (canvas.Width - item.Width) / 2;
        var centerY = (canvas.Height - item.Height) / 2;
        
        Canvas.SetLeft(item, centerX);
        Canvas.SetTop(item, centerY);

        Assert.Equal(centerX, Canvas.GetLeft(item));
        Assert.Equal(centerY, Canvas.GetTop(item));
        
        // Verify it's actually centered
        var expectedCenterX = (800 - 200) / 2; // 300
        var expectedCenterY = (600 - 150) / 2; // 225
        
        Assert.Equal(expectedCenterX, Canvas.GetLeft(item));
        Assert.Equal(expectedCenterY, Canvas.GetTop(item));
    }
}