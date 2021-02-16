
namespace Dock.Model.Core
{
    /// <summary>
    /// Host adapter contract for the <see cref="IDockWindow"/>.
    /// </summary>
    public interface IHostAdapter
    {
        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Save()"/> method.
        /// </summary>
        void Save();

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Present(bool)"/> method.
        /// </summary>
        void Present(bool isDialog);

        /// <summary>
        /// Implementation of the <see cref="IDockWindow.Exit()"/> method.
        /// </summary>
        void Exit();
    }
}
