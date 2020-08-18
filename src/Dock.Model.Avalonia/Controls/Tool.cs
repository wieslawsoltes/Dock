using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class Tool : DockableBase, ITool, IDocument
    {
        /// <summary>
        /// Initializes new instance of the <see cref="Tool"/> class.
        /// </summary>
        public Tool()
        {
            Id = nameof(ITool);
            Title = nameof(ITool);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return this;
        }
    }
}
