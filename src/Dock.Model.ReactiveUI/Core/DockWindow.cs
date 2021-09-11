using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core
{
    /// <summary>
    /// Dock window.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DockWindow : ReactiveObject, IDockWindow
    {
        private readonly IHostAdapter _hostAdapter;
        private string _id;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _topmost;
        private string _title;
        private IDockable? _owner;
        private IFactory? _factory;
        private IRootDock? _layout;
        private IHostWindow? _host;

        /// <summary>
        /// Initializes new instance of the <see cref="DockWindow"/> class.
        /// </summary>
        public DockWindow()
        {
            _id = nameof(IDockWindow);
            _title = nameof(IDockWindow);
            _hostAdapter = new HostAdapter(this);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool Topmost
        {
            get => _topmost;
            set => this.RaiseAndSetIfChanged(ref _topmost, value);
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
        public IRootDock? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IHostWindow? Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
        }

        /// <inheritdoc/>
        public virtual bool OnClose()
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual bool OnMoveDragBegin()
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual void OnMoveDrag()
        {
        }

        /// <inheritdoc/>
        public virtual void OnMoveDragEnd()
        {
        }

        /// <inheritdoc/>
        public void Save()
        {
            _hostAdapter.Save();
        }

        /// <inheritdoc/>
        public void Present(bool isDialog)
        {
            _hostAdapter.Present(isDialog);
        }

        /// <inheritdoc/>
        public void Exit()
        {
            _hostAdapter.Exit();
        }
    }
}
