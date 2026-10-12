namespace Dock.Model.Core;

/// <summary>Creates independent model factories for non-mutating docking previews.</summary>
public interface IDockPreviewFactoryProvider
{
    /// <summary>Creates a factory without the live layout's contexts, windows or event subscribers.</summary>
    /// <remarks>
    /// Preserve layout defaults and docking behavior, including constraints on newly created panes.
    /// A projection creates one isolated factory per participating live factory, so moves between
    /// layouts retain the source and target factories' respective behavior.
    /// Derived factories must opt in explicitly; inherited stock implementations return null.
    /// Return null when the custom behavior cannot be reproduced without live application services.
    /// </remarks>
    /// <returns>An isolated factory used only for copied layout models, or null to retain legacy indicators.</returns>
    IFactory? CreatePreviewFactory();
}
