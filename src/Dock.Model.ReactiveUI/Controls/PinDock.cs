using System.Runtime.Serialization;
using ReactiveUI;
using System;

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
        /// Initializes new instance of the <see cref="PinDock"/> class.
        /// </summary>
        public PinDock()
        {
            Id = nameof(IPinDock);
            Title = nameof(IPinDock);

            this.ObservableForProperty(n => n.IsActive)
                .Subscribe(isActive =>
                {
                    if(AutoHide)
                    {
                        IsExpanded = isActive.Value;
                    }
                });
            this.ObservableForProperty(n => n.AutoHide)
                .Subscribe(autoHide =>
                {
                    IsExpanded = true;
                });
            this.ObservableForProperty(n => n.ActiveDockable)
                .Subscribe(activeDockable =>
                {
                    if(VisibleDockables?.Count == 0)
                    {
                        IsActive = false;
                    }
                });
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.ClonePinDock(this);
        }
    }
}
