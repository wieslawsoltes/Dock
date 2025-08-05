using System;
using System.Collections.Generic;

namespace DockReactiveUIRoutingSample.Models;

public class CrossNavigationContent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DocumentSection
{
    public int SectionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ComparisonResult
{
    public string Category { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
    public string ScoreColor { get; set; } = string.Empty;
}

public class ToolMetric
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double NumericValue { get; set; }
}