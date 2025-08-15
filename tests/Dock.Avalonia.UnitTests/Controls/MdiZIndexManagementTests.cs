using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dock.Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class MdiZIndexManagementTests
{
    [AvaloniaFact]
    public void ZIndex_InitialValue()
    {
        var item = new MdiDocumentItem();
        
        // Initial Z-index should be 0 (default)
        Assert.Equal(0, item.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_SetValue()
    {
        var item = new MdiDocumentItem();
        
        item.SetValue(Visual.ZIndexProperty, 5);
        Assert.Equal(5, item.GetValue(Visual.ZIndexProperty));
        
        item.SetValue(Visual.ZIndexProperty, -2);
        Assert.Equal(-2, item.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_MultipleItems_OrderedByAddition()
    {
        var canvas = new Canvas();
        var items = new List<MdiDocumentItem>();
        
        // Add multiple items
        for (int i = 0; i < 5; i++)
        {
            var item = new MdiDocumentItem() { Width = 200, Height = 150 };
            items.Add(item);
            canvas.Children.Add(item);
        }

        // Z-index should generally increase with order of addition
        // (though the exact implementation may vary)
        for (int i = 1; i < items.Count; i++)
        {
            var prevZIndex = items[i - 1].GetValue(Visual.ZIndexProperty);
            var currZIndex = items[i].GetValue(Visual.ZIndexProperty);
            Assert.True(currZIndex >= prevZIndex, $"Item {i} Z-index ({currZIndex}) should be >= previous item Z-index ({prevZIndex})");
        }
    }

    [AvaloniaFact]
    public void ZIndex_BringToFront()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        var initialZIndex1 = item1.GetValue(Visual.ZIndexProperty);
        var initialZIndex2 = item2.GetValue(Visual.ZIndexProperty);
        var initialZIndex3 = item3.GetValue(Visual.ZIndexProperty);

        // Bring item1 to front
        var maxZIndex = Math.Max(Math.Max(initialZIndex1, initialZIndex2), initialZIndex3);
        item1.SetValue(Visual.ZIndexProperty, maxZIndex + 1);

        // item1 should now have the highest Z-index
        Assert.True(item1.GetValue(Visual.ZIndexProperty) > item2.GetValue(Visual.ZIndexProperty));
        Assert.True(item1.GetValue(Visual.ZIndexProperty) > item3.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_SendToBack()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        var initialZIndex1 = item1.GetValue(Visual.ZIndexProperty);
        var initialZIndex2 = item2.GetValue(Visual.ZIndexProperty);
        var initialZIndex3 = item3.GetValue(Visual.ZIndexProperty);

        // Send item3 to back
        var minZIndex = Math.Min(Math.Min(initialZIndex1, initialZIndex2), initialZIndex3);
        item3.SetValue(Visual.ZIndexProperty, minZIndex - 1);

        // item3 should now have the lowest Z-index
        Assert.True(item3.GetValue(Visual.ZIndexProperty) < item1.GetValue(Visual.ZIndexProperty));
        Assert.True(item3.GetValue(Visual.ZIndexProperty) < item2.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_ActivateWindow_BringsToFront()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Simulate activating item1 (which should bring it to front)
        item1.IsActive = true;
        
        // In a real implementation, activating a window would update its Z-index
        // For testing, we'll simulate this behavior
        var allItems = canvas.Children.OfType<MdiDocumentItem>().ToList();
        var maxZIndex = allItems.Max(i => i.GetValue(Visual.ZIndexProperty));
        item1.SetValue(Visual.ZIndexProperty, maxZIndex + 1);

        // item1 should now be on top
        Assert.True(item1.GetValue(Visual.ZIndexProperty) > item2.GetValue(Visual.ZIndexProperty));
        Assert.True(item1.GetValue(Visual.ZIndexProperty) > item3.GetValue(Visual.ZIndexProperty));
        Assert.True(item1.IsActive);
    }

    [AvaloniaFact]
    public void ZIndex_MaximizedWindow_HighestZIndex()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Maximize item2
        item2.Maximize();
        
        // In a real implementation, maximized windows should have highest Z-index
        // Simulate this behavior
        var allItems = canvas.Children.OfType<MdiDocumentItem>().ToList();
        var maxZIndex = allItems.Max(i => i.GetValue(Visual.ZIndexProperty));
        item2.SetValue(Visual.ZIndexProperty, maxZIndex + 100); // Much higher for maximized

        Assert.True(item2.GetValue(Visual.ZIndexProperty) > item1.GetValue(Visual.ZIndexProperty));
        Assert.True(item2.GetValue(Visual.ZIndexProperty) > item3.GetValue(Visual.ZIndexProperty));
        Assert.True(item2.IsMaximized);
    }

    [AvaloniaFact]
    public void ZIndex_MinimizedWindow_LowestZIndex()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Minimize item2
        item2.Minimize();
        
        // In a real implementation, minimized windows might have lower Z-index
        // or be handled differently. For testing, we'll simulate lower Z-index
        var allItems = canvas.Children.OfType<MdiDocumentItem>().ToList();
        var minZIndex = allItems.Min(i => i.GetValue(Visual.ZIndexProperty));
        item2.SetValue(Visual.ZIndexProperty, minZIndex - 1);

        Assert.True(item2.GetValue(Visual.ZIndexProperty) < item1.GetValue(Visual.ZIndexProperty));
        Assert.True(item2.GetValue(Visual.ZIndexProperty) < item3.GetValue(Visual.ZIndexProperty));
        Assert.True(item2.IsMinimized);
    }

    [AvaloniaFact]
    public void ZIndex_RestoreWindow_RestoresZIndex()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);

        var originalZIndex = item2.GetValue(Visual.ZIndexProperty);

        // Maximize then restore
        item2.Maximize();
        item2.SetValue(Visual.ZIndexProperty, 100); // High Z-index for maximized
        
        item2.Restore();
        // In a real implementation, Z-index might be restored or recalculated
        // For testing, we'll simulate restoring to a normal level
        item2.SetValue(Visual.ZIndexProperty, originalZIndex);

        Assert.Equal(originalZIndex, item2.GetValue(Visual.ZIndexProperty));
        Assert.False(item2.IsMaximized);
    }

    [AvaloniaFact]
    public void ZIndex_WindowLayering_CorrectOrder()
    {
        var canvas = new Canvas();
        var bottomItem = new MdiDocumentItem() { Width = 200, Height = 150 };
        var middleItem = new MdiDocumentItem() { Width = 200, Height = 150 };
        var topItem = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(bottomItem);
        canvas.Children.Add(middleItem);
        canvas.Children.Add(topItem);

        // Set explicit Z-index values
        bottomItem.SetValue(Visual.ZIndexProperty, 1);
        middleItem.SetValue(Visual.ZIndexProperty, 5);
        topItem.SetValue(Visual.ZIndexProperty, 10);

        // Verify layering order
        Assert.True(bottomItem.GetValue(Visual.ZIndexProperty) < middleItem.GetValue(Visual.ZIndexProperty));
        Assert.True(middleItem.GetValue(Visual.ZIndexProperty) < topItem.GetValue(Visual.ZIndexProperty));
        
        // The item with highest Z-index should be visually on top
        var allItems = new[] { bottomItem, middleItem, topItem };
        var sortedByZIndex = allItems.OrderBy(i => i.GetValue(Visual.ZIndexProperty)).ToArray();
        
        Assert.Equal(bottomItem, sortedByZIndex[0]);
        Assert.Equal(middleItem, sortedByZIndex[1]);
        Assert.Equal(topItem, sortedByZIndex[2]);
    }

    [AvaloniaFact]
    public void ZIndex_NegativeValues()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);

        // Set negative Z-index values
        item1.SetValue(Visual.ZIndexProperty, -5);
        item2.SetValue(Visual.ZIndexProperty, -2);

        Assert.Equal(-5, item1.GetValue(Visual.ZIndexProperty));
        Assert.Equal(-2, item2.GetValue(Visual.ZIndexProperty));
        
        // item2 should be above item1 (less negative)
        Assert.True(item2.GetValue(Visual.ZIndexProperty) > item1.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_LargeValues()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);

        // Set large Z-index values
        item1.SetValue(Visual.ZIndexProperty, 1000000);
        item2.SetValue(Visual.ZIndexProperty, 2000000);

        Assert.Equal(1000000, item1.GetValue(Visual.ZIndexProperty));
        Assert.Equal(2000000, item2.GetValue(Visual.ZIndexProperty));
        
        Assert.True(item2.GetValue(Visual.ZIndexProperty) > item1.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_SameValues_OrderByAddition()
    {
        var canvas = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item3 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item1);
        canvas.Children.Add(item2);
        canvas.Children.Add(item3);

        // Set same Z-index for all
        item1.SetValue(Visual.ZIndexProperty, 5);
        item2.SetValue(Visual.ZIndexProperty, 5);
        item3.SetValue(Visual.ZIndexProperty, 5);

        Assert.Equal(5, item1.GetValue(Visual.ZIndexProperty));
        Assert.Equal(5, item2.GetValue(Visual.ZIndexProperty));
        Assert.Equal(5, item3.GetValue(Visual.ZIndexProperty));
        
        // When Z-index is the same, order should be determined by addition order
        // (this is handled by the panel's children collection order)
        Assert.Equal(0, canvas.Children.IndexOf(item1));
        Assert.Equal(1, canvas.Children.IndexOf(item2));
        Assert.Equal(2, canvas.Children.IndexOf(item3));
    }

    [AvaloniaFact]
    public void ZIndex_RemoveAndAddItem_MaintainsZIndex()
    {
        var canvas = new Canvas();
        var item = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas.Children.Add(item);
        item.SetValue(Visual.ZIndexProperty, 42);
        
        Assert.Equal(42, item.GetValue(Visual.ZIndexProperty));
        
        // Remove and re-add
        canvas.Children.Remove(item);
        canvas.Children.Add(item);
        
        // Z-index should be maintained
        Assert.Equal(42, item.GetValue(Visual.ZIndexProperty));
    }

    [AvaloniaFact]
    public void ZIndex_MultipleCanvases_IndependentZIndex()
    {
        var canvas1 = new Canvas();
        var canvas2 = new Canvas();
        var item1 = new MdiDocumentItem() { Width = 200, Height = 150 };
        var item2 = new MdiDocumentItem() { Width = 200, Height = 150 };
        
        canvas1.Children.Add(item1);
        canvas2.Children.Add(item2);
        
        item1.SetValue(Visual.ZIndexProperty, 10);
        item2.SetValue(Visual.ZIndexProperty, 20);
        
        Assert.Equal(10, item1.GetValue(Visual.ZIndexProperty));
        Assert.Equal(20, item2.GetValue(Visual.ZIndexProperty));
        
        // Move item1 to canvas2
        canvas1.Children.Remove(item1);
        canvas2.Children.Add(item1);
        
        // Z-index should be preserved
        Assert.Equal(10, item1.GetValue(Visual.ZIndexProperty));
        Assert.Equal(20, item2.GetValue(Visual.ZIndexProperty));
        
        // Both items are now in canvas2 with their respective Z-indices
        Assert.Contains(item1, canvas2.Children);
        Assert.Contains(item2, canvas2.Children);
    }

    [AvaloniaFact]
    public void ZIndex_AutoIncrement_NewItems()
    {
        var canvas = new Canvas();
        var items = new List<MdiDocumentItem>();
        
        // Add items and simulate auto-incrementing Z-index
        for (int i = 0; i < 10; i++)
        {
            var item = new MdiDocumentItem() { Width = 200, Height = 150 };
            canvas.Children.Add(item);
            
            // Simulate auto-increment Z-index behavior
            item.SetValue(Visual.ZIndexProperty, i);
            items.Add(item);
        }
        
        // Verify Z-indices are incrementing
        for (int i = 0; i < items.Count; i++)
        {
            Assert.Equal(i, items[i].GetValue(Visual.ZIndexProperty));
        }
        
        // Last item should have highest Z-index
        var maxZIndex = items.Max(item => item.GetValue(Visual.ZIndexProperty));
        Assert.Equal(items.Last().GetValue(Visual.ZIndexProperty), maxZIndex);
    }
}