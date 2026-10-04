namespace Dock.Model.Core;

/// <summary>Creates independent model factories for non-mutating docking previews.</summary>
public interface IDockPreviewFactoryProvider
{
    /// <summary>Creates a factory without the live layout's contexts, windows or event subscribers.</summary>
    /// <remarks>
    /// Preserve layout defaults and docking behavior, including constraints on newly created panes.
    /// A projection creates one isolated factory per participating live factory, so moves between
    /// layouts retain the source and target factories' respective behavior.
    /// </remarks>
    /// <returns>An isolated factory used only for copied layout models.</returns>
    IFactory CreatePreviewFactory();
}
