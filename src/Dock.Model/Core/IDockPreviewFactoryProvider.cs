namespace Dock.Model.Core;

/// <summary>Creates independent model factories for non-mutating docking previews.</summary>
public interface IDockPreviewFactoryProvider
{
    /// <summary>Creates a factory without the live layout's contexts, windows or event subscribers.</summary>
    /// <returns>An isolated factory used only for copied layout models.</returns>
    IFactory CreatePreviewFactory();
}
