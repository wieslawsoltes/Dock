using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToolDock : DockBase, IToolDock
    {
        private Alignment _alignment = Alignment.Unset;
        private bool _isExpanded = false;
        private bool _autoHide = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public Alignment Alignment
        {
            get => _alignment;
            set => this.RaiseAndSetIfChanged(ref _alignment, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool AutoHide
        {
            get => _autoHide;
            set => this.RaiseAndSetIfChanged(ref _autoHide, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="ToolDock"/> class.
        /// </summary>
        public ToolDock()
        {
            Id = nameof(IToolDock);
            Title = nameof(IToolDock);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.CloneToolDock(this);
        }
    }
}
