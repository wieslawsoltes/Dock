// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.UnitTests.Helpers;

/// <summary>
/// Unit tests for <see cref="DockDataTemplates"/>.
/// </summary>
public class DockDataTemplatesTests
{
    /// <summary>
    /// Tests that all expected dock types have corresponding DataTemplates.
    /// </summary>
    [Fact]
    public void Templates_ShouldSupportAllDockTypes()
    {
        // Arrange - Automatically discover all dock-related interface types from assemblies
        var expectedTypes = GetAllDockInterfaceTypes();

        // Act
        var templates = new DockDataTemplates().ToList();
        var supportedTypes = templates
            .Select(GetDataTemplateType)
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
                                    $"Add it to DockDataTemplates.axaml");
        }
        
        // Verify we don't have any unexpected templates
        var extraTypes = supportedTypes.Except(expectedTypes).ToList();
        Assert.Empty(extraTypes.Select(t => t?.Name));
    }

    /// <summary>
    /// Tests that no DataTemplate is created more than once.
    /// </summary>
    [Fact]
    public void Templates_ShouldNotHaveDuplicateTemplates()
    {
        // Act
        var templates = new DockDataTemplates().ToList();

        // Assert
        var dataTypes = templates
            .Select(GetDataTemplateType)
            .Where(t => t != null)
            .ToList();

        var distinctDataTypes = dataTypes.Distinct().ToList();
        
        Assert.Equal(dataTypes.Count, distinctDataTypes.Count);
    }

    /// <summary>
    /// Tests that all templates declare the type they match.
    /// </summary>
    [Fact]
    public void Templates_ShouldDeclareTheirDataTypes()
    {
        // Act
        var templates = new DockDataTemplates().ToList();

        // Assert
        Assert.All(templates, template => Assert.NotNull(Assert.IsAssignableFrom<ITypedDataTemplate>(template).DataType));
    }

    /// <summary>
    /// Tests that ProportionalStackPanelSplitter is created correctly.
    /// </summary>
    [Fact]
    public void Templates_ProportionalSplitter_ShouldCreateCorrectControl()
    {
        // Act
        var templates = new DockDataTemplates().ToList();
        var proportionalTemplate = templates
            .FirstOrDefault(t => GetDataTemplateType(t) == typeof(IProportionalDockSplitter));

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
    public void Templates_GridSplitter_ShouldHaveTemplate()
    {
        // Act
        var templates = new DockDataTemplates().ToList();
        var gridTemplate = templates
            .FirstOrDefault(t => GetDataTemplateType(t) == typeof(IGridDockSplitter));

        // Assert
        Assert.NotNull(gridTemplate);
        
        // Verify the template exists but don't create the control due to platform dependencies
        Assert.Equal(typeof(IGridDockSplitter), GetDataTemplateType(gridTemplate));
    }

    /// <summary>
    /// Tests that all content types that should be supported are covered.
    /// This test ensures that if new dock content types are added to the model,
    /// they are also added to the DataTemplate helper.
    /// </summary>
    [Fact]
    public void Templates_ShouldCoverAllDockContentTypes()
    {
        // This test is covered by Templates_ShouldSupportAllDockTypes
        // which automatically discovers types from assemblies.
        // Keeping this test for backwards compatibility and explicit coverage verification.
        
        // Arrange - Get dynamically discovered dock content types
        var allDockContentTypes = GetAllDockInterfaceTypes();

        // Act
        var templates = new DockDataTemplates().ToList();
        var supportedTypes = templates
            .Select(GetDataTemplateType)
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

    private static Type? GetDataTemplateType(IDataTemplate template)
    {
        return (template as ITypedDataTemplate)?.DataType;
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
    /// Determines if an interface type should have a corresponding DataTemplate
    /// by checking for the RequiresDataTemplateAttribute.
    /// </summary>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>True if the type should have a DataTemplate, false otherwise.</returns>
    private static bool ShouldHaveDataTemplate(Type interfaceType)
    {
        return interfaceType.GetCustomAttribute<RequiresDataTemplateAttribute>() != null;
    }
}
