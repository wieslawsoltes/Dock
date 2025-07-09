using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Adapter for <see cref="IProportionalDockSplitter"/> operations.
/// </summary>
public interface IProportionalDockSplitterAdapter
{
    /// <summary>
    /// Resets proportions of neighbours of specified splitter.
    /// </summary>
    /// <param name="splitter">Splitter instance.</param>
    void ResetProportion(IProportionalDockSplitter splitter);

    /// <summary>
    /// Sets proportion of element preceding the splitter.
    /// </summary>
    /// <param name="splitter">Splitter instance.</param>
    /// <param name="proportion">Desired proportion.</param>
    void SetProportion(IProportionalDockSplitter splitter, double proportion);
}
