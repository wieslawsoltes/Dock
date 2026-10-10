namespace DockAvalonDockSample.Services;

/// <summary>Layout actions available to the sample's presentation layer.</summary>
public interface ILayoutWorkspace
{
    /// <summary>Saves the current layout and returns a status message.</summary>
    string Save();
    /// <summary>Restores the saved layout and returns a status message.</summary>
    string Load();
    /// <summary>Hides the journal tool and returns a status message.</summary>
    string HideJournal();
    /// <summary>Shows the journal tool and returns a status message.</summary>
    string ShowJournal();
}
