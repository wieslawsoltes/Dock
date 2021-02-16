using Dock.Model.Controls;

namespace Dock.Model.Core
{
    /// <summary>
    /// Navigate adapter contract for the <see cref="IDock"/>.
    /// </summary>
    public interface INavigateAdapter
    {
        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Navigates to the most recent entry in back navigation history, if there is one.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Navigate to the most recent entry in forward navigation history, if there is one.
        /// </summary>
        void GoForward();

        /// <summary>
        /// Implementation of the <see cref="IDock.Navigate"/> method.
        /// </summary>
        /// <param name="root">An object that contains the content to navigate to.</param>
        /// <param name="bSnapshot">The flag indicating whether to make snapshot.</param>
        void Navigate(object root, bool bSnapshot);

        /// <summary>
        /// Implementation of the <see cref="IRootDock.ShowWindows"/> method.
        /// </summary>
        void ShowWindows();

        /// <summary>
        /// Implementation of the <see cref="IRootDock.ExitWindows"/> method.
        /// </summary>
        void ExitWindows();

        /// <summary>
        /// Implementation of the <see cref="IDock.Close"/> method.
        /// </summary>
        void Close();
    }
}
