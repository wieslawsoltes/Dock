using System.Runtime.Serialization;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core
{
    /// <summary>
    /// Dockable base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockableBase : ReactiveObject, IDockable
    {
        private string _id = string.Empty;
        private string _title = string.Empty;
        private object? _context;
        private IDockable? _owner;
        private IFactory? _factory;
        private bool _canClose = true;
        private bool _canPin = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public object? Context
        {
            get => _context;
            set => this.RaiseAndSetIfChanged(ref _context, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockable? Owner
        {
            get => _owner;
            set => this.RaiseAndSetIfChanged(ref _owner, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IFactory? Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanClose
        {
            get => _canClose;
            set => this.RaiseAndSetIfChanged(ref _canClose, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanPin
        {
            get => _canPin;
            set => this.RaiseAndSetIfChanged(ref _canPin, value);
        }

        /// <inheritdoc/>
        public virtual bool OnClose()
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual void OnSelected()
        {
        }

        /// <inheritdoc/>
        public abstract IDockable? Clone();
    }
}
