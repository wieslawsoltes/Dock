// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using ReactiveUI;

namespace Dock.Model
{
    /// <summary>
    /// Dock window.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DockWindow : ReactiveObject, IDockWindow
    {
        private IHostAdapter _hostAdapter;
        private string _id;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _topmost;
        private string _title;
        private IView _owner;
        private IDockFactory _factory;
        private IDock _layout;
        private IDockHost _host;

        /// <summary>
        /// Initializes new instance of the <see cref="DockWindow"/> class.
        /// </summary>
        public DockWindow()
        {
            _hostAdapter = new HostAdapter(this);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
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
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool Topmost
        {
            get => _topmost;
            set => this.RaiseAndSetIfChanged(ref _topmost, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IView Owner
        {
            get => _owner;
            set => this.RaiseAndSetIfChanged(ref _owner, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDock Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockHost Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
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
