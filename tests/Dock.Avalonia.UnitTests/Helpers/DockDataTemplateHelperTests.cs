// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Helpers;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.UnitTests.Helpers;

/// <summary>
/// Unit tests for <see cref="DockDataTemplateHelper"/>.
/// </summary>
public class DockDataTemplateHelperTests
{
    /// <summary>
    /// Tests that all expected dock types have corresponding DataTemplates.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_ShouldSupportAllDockTypes()
    {
        // Arrange - Automatically discover all dock-related interface types from assemblies
        var expectedTypes = GetAllDockInterfaceTypes();

        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();
        var supportedTypes = templates
            .Select(GetFuncDataTemplateType)
            .Where(t => t != null)
            .ToHashSet();

        // Assert
        Assert.NotEmpty(templates);
        Assert.NotEmpty(expectedTypes);

        // Verify each expected type has a corresponding template
        foreach (var expectedType in expectedTypes)
        {
            var hasTemplate = supportedTypes.Contains(expectedType);
            Assert.True(hasTemplate, $"Missing DataTemplate for type {expectedType.Name}. " +
                                    $"Add it to DockDataTemplateHelper.CreateDefaultDataTemplates()");
        }
        
        // Verify we don't have any unexpected templates
        var extraTypes = supportedTypes.Except(expectedTypes).ToList();
        Assert.Empty(extraTypes.Select(t => t?.Name));
    }

    /// <summary>
    /// Tests that no DataTemplate is created more than once.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_ShouldNotHaveDuplicateTemplates()
    {
        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();

        // Assert
        var dataTypes = templates
            .Select(GetFuncDataTemplateType)
            .Where(t => t != null)
            .ToList();

        var distinctDataTypes = dataTypes.Distinct().ToList();
        
        Assert.Equal(dataTypes.Count, distinctDataTypes.Count);
    }

    /// <summary>
    /// Tests that all templates are of the expected FuncDataTemplate type.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_ShouldReturnFuncDataTemplates()
    {
        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();

        // Assert
        Assert.All(templates, template => 
        {
            Assert.True(IsFuncDataTemplate(template), $"Template is not a FuncDataTemplate: {template.GetType()}");
        });
    }

    /// <summary>
    /// Tests that ProportionalStackPanelSplitter is created correctly.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_ProportionalSplitter_ShouldCreateCorrectControl()
    {
        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();
        var proportionalTemplate = templates
            .FirstOrDefault(t => GetFuncDataTemplateType(t) == typeof(IProportionalDockSplitter));

        // Assert
        Assert.NotNull(proportionalTemplate);
        
        var control = proportionalTemplate.Build(null);
        Assert.IsType<ProportionalStackPanelSplitter>(control);
    }

    /// <summary>
    /// Tests that GridSplitter DataTemplate exists.
    /// Note: Control creation is skipped due to platform dependencies in unit tests.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_GridSplitter_ShouldHaveTemplate()
    {
        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();
        var gridTemplate = templates
            .FirstOrDefault(t => GetFuncDataTemplateType(t) == typeof(IGridDockSplitter));

        // Assert
        Assert.NotNull(gridTemplate);
        
        // Verify the template exists but don't create the control due to platform dependencies
        Assert.True(IsFuncDataTemplate(gridTemplate));
        Assert.Equal(typeof(IGridDockSplitter), GetFuncDataTemplateType(gridTemplate));
    }

    /// <summary>
    /// Tests that all content types that should be supported are covered.
    /// This test ensures that if new dock content types are added to the model,
    /// they are also added to the DataTemplate helper.
    /// </summary>
    [Fact]
    public void CreateDefaultDataTemplates_ShouldCoverAllDockContentTypes()
    {
        // This test is covered by CreateDefaultDataTemplates_ShouldSupportAllDockTypes
        // which automatically discovers types from assemblies.
        // Keeping this test for backwards compatibility and explicit coverage verification.
        
        // Arrange - Get dynamically discovered dock content types
        var allDockContentTypes = GetAllDockInterfaceTypes();

        // Act
        var templates = DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();
        var supportedTypes = templates
            .Select(GetFuncDataTemplateType)
            .Where(t => t != null)
            .ToHashSet();

        // Assert
        foreach (var contentType in allDockContentTypes)
        {
            Assert.Contains(contentType, supportedTypes);
        }

        // Ensure we're not missing any types or have extras
        Assert.Equal(allDockContentTypes.Count, supportedTypes.Count);
    }

    /// <summary>
    /// Helper method to extract the type parameter from a FuncDataTemplate&lt;T&gt;.
    /// </summary>
    /// <param name="template">The template to examine.</param>
    /// <returns>The type parameter T, or null if not a FuncDataTemplate.</returns>
    private static Type? GetFuncDataTemplateType(IDataTemplate template)
    {
        var templateType = template.GetType();
        
        // Check if this is a FuncDataTemplate<T>
        if (templateType.IsGenericType && 
            templateType.GetGenericTypeDefinition() == typeof(FuncDataTemplate<>))
        {
            return templateType.GetGenericArguments()[0];
        }
        
        return null;
    }

    /// <summary>
    /// Helper method to check if a template is a FuncDataTemplate.
    /// </summary>
    /// <param name="template">The template to check.</param>
    /// <returns>True if it's a FuncDataTemplate, false otherwise.</returns>
    private static bool IsFuncDataTemplate(IDataTemplate template)
    {
        var templateType = template.GetType();
        return templateType.IsGenericType && 
               templateType.GetGenericTypeDefinition() == typeof(FuncDataTemplate<>);
    }

    /// <summary>
    /// Automatically discovers all dock-related interface types from the Dock.Model assembly.
    /// This ensures the test automatically detects when new dock types are added.
    /// </summary>
    /// <returns>A collection of dock interface types that should have DataTemplates.</returns>
    private static List<Type> GetAllDockInterfaceTypes()
    {
        var dockModelAssembly = typeof(IDockable).Assembly;
        var dockTypes = new List<Type>();

        // Get all interfaces from Dock.Model that are dock-related
        var allInterfaces = dockModelAssembly.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic)
            .ToList();

        foreach (var interfaceType in allInterfaces)
        {
            // Include interfaces that should have DataTemplates:
            // 1. Content interfaces (IDocumentContent, IToolContent)
            // 2. Splitter interfaces (IProportionalDockSplitter, IGridDockSplitter)  
            // 3. Dock container interfaces that inherit from IDock
            if (ShouldHaveDataTemplate(interfaceType))
            {
                dockTypes.Add(interfaceType);
            }
        }

        return dockTypes.OrderBy(t => t.Name).ToList();
    }

    /// <summary>
    /// Determines if an interface type should have a corresponding DataTemplate.
    /// </summary>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>True if the type should have a DataTemplate, false otherwise.</returns>
    private static bool ShouldHaveDataTemplate(Type interfaceType)
    {
        // Content types that need DataTemplates
        if (interfaceType == typeof(IDocumentContent) || 
            interfaceType == typeof(IToolContent))
        {
            return true;
        }

        // Splitter types that need DataTemplates
        if (interfaceType == typeof(IProportionalDockSplitter) || 
            interfaceType == typeof(IGridDockSplitter))
        {
            return true;
        }

        // Dock container types that need DataTemplates
        // These are concrete dock types that inherit from IDock but are not base interfaces
        if (typeof(IDock).IsAssignableFrom(interfaceType) && 
            interfaceType != typeof(IDock) && 
            interfaceType != typeof(IDockable))
        {
            // Exclude abstract base interfaces and include only concrete dock types
            var isDockContainerType = interfaceType.Name.EndsWith("Dock") && 
                                    !interfaceType.Name.Contains("Content") &&
                                    interfaceType.Namespace == "Dock.Model.Controls";
            
            if (isDockContainerType)
            {
                return true;
            }
        }

        return false;
    }
}
