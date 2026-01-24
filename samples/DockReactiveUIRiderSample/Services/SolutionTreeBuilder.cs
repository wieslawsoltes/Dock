using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DockReactiveUIRiderSample.Models;
using Microsoft.CodeAnalysis;

namespace DockReactiveUIRiderSample.Services;

public static class SolutionTreeBuilder
{
    public static SolutionItemViewModel Build(Solution solution)
    {
        var solutionName = solution.FilePath is null
            ? "Solution"
            : Path.GetFileNameWithoutExtension(solution.FilePath);
        var root = new SolutionItemViewModel(SolutionItemKind.Solution, solutionName, solution.FilePath);
        var solutionDirectory = solution.FilePath is null
            ? null
            : Path.GetDirectoryName(solution.FilePath);

        foreach (var project in solution.Projects.OrderBy(p => p.Name))
        {
            var projectNode = new SolutionItemViewModel(SolutionItemKind.Project, project.Name, project.FilePath);
            root.Children.Add(projectNode);

            var projectDirectory = project.FilePath is null
                ? solutionDirectory
                : Path.GetDirectoryName(project.FilePath);

            AddDocuments(projectNode, projectDirectory, project.Documents);
            AddDocuments(projectNode, projectDirectory, project.AdditionalDocuments);
            AddDocuments(projectNode, projectDirectory, project.AnalyzerConfigDocuments);
        }

        root.IsExpanded = true;
        return root;
    }

    private static void AddDocuments(
        SolutionItemViewModel projectNode,
        string? projectDirectory,
        IEnumerable<TextDocument> documents)
    {
        foreach (var document in documents)
        {
            if (document.FilePath is null)
            {
                continue;
            }

            var relativePath = GetRelativePath(projectDirectory, document.FilePath);
            AddPath(projectNode, relativePath, document.FilePath);
        }
    }

    private static string GetRelativePath(string? basePath, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return Path.GetFileName(fullPath);
        }

        var relativePath = Path.GetRelativePath(basePath, fullPath);
        return relativePath.StartsWith("..", StringComparison.Ordinal)
            ? Path.GetFileName(fullPath)
            : relativePath;
    }

    private static void AddPath(SolutionItemViewModel root, string relativePath, string fullPath)
    {
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var segments = relativePath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        var current = root;

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isLeaf = i == segments.Length - 1;

            if (isLeaf)
            {
                current.Children.Add(new SolutionItemViewModel(SolutionItemKind.Document, segment, fullPath));
                continue;
            }

            var folder = current.Children.FirstOrDefault(child =>
                child.Kind == SolutionItemKind.Folder &&
                string.Equals(child.Name, segment, StringComparison.OrdinalIgnoreCase));

            if (folder is null)
            {
                folder = new SolutionItemViewModel(SolutionItemKind.Folder, segment, null);
                current.Children.Add(folder);
            }

            current = folder;
        }
    }
}
