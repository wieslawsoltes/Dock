using System.ComponentModel;
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Pin dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class PinDock : DockBase, IPinDock
    {
        private Alignment _alignment = Alignment.Unset;
        private bool _isExpanded = false;
        private bool _autoHide = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public Alignment Alignment
        {
            get => _alignment;
            set => RaiseAndSetIfChanged(ref _alignment, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool AutoHide
        {
            get => _autoHide;
            set => RaiseAndSetIfChanged(ref _autoHide, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="PinDock"/> class.
        /// </summary>
        public PinDock()
        {
            Id = nameof(IPinDock);
            Title = nameof(IPinDock);

            PropertyChanged += PinDock_PropertyChanged;
        }

        private void PinDock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsActive):
                    if (AutoHide)
                    {
                        IsExpanded = IsActive;
                    }
                    break;
                case nameof(AutoHide):
                    IsExpanded = true;
                    break;
                case nameof(ActiveDockable):
                    if (VisibleDockables?.Count == 0)
                    {
                        IsActive = false;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.ClonePinDock(this);
        }
    }
}
