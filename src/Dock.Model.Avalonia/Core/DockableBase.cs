// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model
{
    /// <summary>
    /// Dockable base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockableBase : StyledElement, IDockable
    {
        /// <summary>
        /// Defines the <see cref="Id"/> property.
        /// </summary>
        public static readonly StyledProperty<string> IdProperty = 
            AvaloniaProperty.Register<DockableBase, string>(nameof(Id));

        /// <summary>
        /// Defines the <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<DockableBase, string>(nameof(Title));

        /// <summary>
        /// Defines the <see cref="Context"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContextProperty =
            AvaloniaProperty.Register<DockableBase, object>(nameof(Context));

        /// <summary>
        /// Defines the <see cref="Owner"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockable> OwnerProperty =
            AvaloniaProperty.Register<DockableBase, IDockable>(nameof(Owner));

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Id
        {
            get { return GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
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
        public IDockable Owner
        {
            get { return GetValue(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
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
    }
}
