using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DockReactiveUIRiderSample.Services;

public sealed record SolutionLoadResult(Solution? Solution, IReadOnlyList<WorkspaceDiagnostic> Diagnostics);

public sealed class SolutionLoader
{
    private MSBuildWorkspace? _workspace;

    public async Task<SolutionLoadResult> LoadAsync(string solutionPath, CancellationToken cancellationToken)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        _workspace?.Dispose();

        var diagnostics = new List<WorkspaceDiagnostic>();
        var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
        {
            ["UseSharedCompilation"] = "false"
        });
        workspace.RegisterWorkspaceFailedHandler(args => diagnostics.Add(args.Diagnostic));

        var solution = await workspace.OpenSolutionAsync(solutionPath, progress: null, cancellationToken);
        _workspace = workspace;

        return new SolutionLoadResult(solution, diagnostics);
    }

    public void Clear()
    {
        _workspace?.Dispose();
        _workspace = null;
    }
}
