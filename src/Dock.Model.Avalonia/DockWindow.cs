// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model
{
    /// <summary>
    /// Dock window.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DockWindow : AvaloniaObject, IDockWindow
    {
        private IHostAdapter _hostAdapter;

        /// <summary>
        /// Defines the <see cref="Id"/> property.
        /// </summary>
        public static readonly StyledProperty<string> IdProperty =
            AvaloniaProperty.Register<DockWindow, string>(nameof(IdProperty));

        /// <summary>
        /// Defines the <see cref="X"/> property.
        /// </summary>
        public static readonly StyledProperty<double> XProperty =
            AvaloniaProperty.Register<DockWindow, double>(nameof(XProperty));

        /// <summary>
        /// Defines the <see cref="Y"/> property.
        /// </summary>
        public static readonly StyledProperty<double> YProperty =
            AvaloniaProperty.Register<DockWindow, double>(nameof(YProperty));

        /// <summary>
        /// Defines the <see cref="Width"/> property.
        /// </summary>
        public static readonly StyledProperty<double> WidthProperty =
            AvaloniaProperty.Register<DockWindow, double>(nameof(WidthProperty));

        /// <summary>
        /// Defines the <see cref="Height"/> property.
        /// </summary>
        public static readonly StyledProperty<double> HeightProperty =
            AvaloniaProperty.Register<DockWindow, double>(nameof(HeightProperty));

        /// <summary>
        /// Defines the <see cref="Topmost"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> TopmostProperty =
            AvaloniaProperty.Register<DockWindow, bool>(nameof(TopmostProperty));

        /// <summary>
        /// Defines the <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<DockWindow, string>(nameof(TitleProperty));

        /// <summary>
        /// Defines the <see cref="Context"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContextProperty =
            AvaloniaProperty.Register<DockWindow, object>(nameof(ContextProperty));

        /// <summary>
        /// Defines the <see cref="Owner"/> property.
        /// </summary>
        public static readonly StyledProperty<IView> OwnerProperty =
            AvaloniaProperty.Register<DockWindow, IView>(nameof(OwnerProperty));

        /// <summary>
        /// Defines the <see cref="Factory"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockFactory> FactoryProperty =
            AvaloniaProperty.Register<DockWindow, IDockFactory>(nameof(FactoryProperty));

        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockWindow, IDock>(nameof(LayoutProperty));

        /// <summary>
        /// Defines the <see cref="Host"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockHost> HostProperty =
            AvaloniaProperty.Register<DockWindow, IDockHost>(nameof(HostProperty));

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Id
        {
            get { return GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double X
        {
            get { return GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Y
        {
            get { return GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Width
        {
            get { return GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = true, EmitDefaultValue = true)]
        public double Height
        {
            get { return GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool Topmost
        {
            get { return GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public object Context
        {
            get { return GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IView Owner
        {
            get { return GetValue(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockFactory Factory
        {
            get { return GetValue(FactoryProperty); }
            set { SetValue(FactoryProperty, value); }
        }

        /// <inheritdoc/>
        [Content]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDock Layout
        {
            get { return GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockHost Host
        {
            get { return GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        /// <summary>
        /// Initializes new instance of the <see cref="DockWindow"/> class.
        /// </summary>
        public DockWindow()
        {
            _hostAdapter = new HostAdapter(this);
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
