using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class ContentTests
{
    [AvaloniaFact]
    public void Tool_Content_With_Function_Returning_Control_Should_Work()
    {
        // Arrange
        var testControl = new TextBlock { Text = "Test Content" };
        var tool = new Tool
        {
            Id = "TestTool",
            Title = "Test Tool",
            Content = new Func<IServiceProvider, object>(_ => testControl)
        };

        // Act
        var result = tool.Build();

        // Assert
        Assert.NotNull(result);
        Assert.Same(testControl, result);
    }

    [AvaloniaFact]
    public void Tool_Content_With_Direct_Control_Should_Work()
    {
        // Arrange
        var testControl = new TextBlock { Text = "Test Content" };
        var tool = new Tool
        {
            Id = "TestTool",
            Title = "Test Tool",
            Content = testControl
        };

        // Act
        var result = tool.Build();

        // Assert
        Assert.NotNull(result);
        Assert.Same(testControl, result);
    }

    [AvaloniaFact]
    public void Document_Content_With_Function_Returning_Control_Should_Work()
    {
        // Arrange
        var testControl = new TextBox { Text = "Test Document Content" };
        var document = new Document
        {
            Id = "TestDoc",
            Title = "Test Document",
            Content = new Func<IServiceProvider, object>(_ => testControl)
        };

        // Act
        var result = document.Build();

        // Assert
        Assert.NotNull(result);
        Assert.Same(testControl, result);
    }

    [AvaloniaFact]
    public void Document_Content_With_Direct_Control_Should_Work()
    {
        // Arrange
        var testControl = new TextBox { Text = "Test Document Content" };
        var document = new Document
        {
            Id = "TestDoc",
            Title = "Test Document",
            Content = testControl
        };

        // Act
        var result = document.Build();

        // Assert
        Assert.NotNull(result);
        Assert.Same(testControl, result);
    }

    [AvaloniaFact]
    public void Tool_Content_With_Function_Returning_UserControl_Should_Work()
    {
        // Arrange
        var userControl = new UserControl { Content = new TextBlock { Text = "UserControl Content" } };
        var tool = new Tool
        {
            Id = "TestTool",
            Title = "Test Tool",
            Content = new Func<IServiceProvider, object>(_ => userControl)
        };

        // Act
        var result = tool.Build();

        // Assert
        Assert.NotNull(result);
        Assert.Same(userControl, result);
    }

    [AvaloniaFact]
    public void Document_Content_With_Null_Should_Return_Null()
    {
        // Arrange
        var document = new Document
        {
            Id = "TestDoc",
            Title = "Test Document",
            Content = null
        };

        // Act
        var result = document.Build();

        // Assert
        Assert.Null(result);
    }

    [AvaloniaFact]
    public void Tool_Content_With_Function_Returning_Null_Should_Return_Null()
    {
        // Arrange
        var tool = new Tool
        {
            Id = "TestTool",
            Title = "Test Tool",
            Content = new Func<IServiceProvider, object>(_ => null!)
        };

        // Act
        var result = tool.Build();

        // Assert
        Assert.Null(result);
    }
}
