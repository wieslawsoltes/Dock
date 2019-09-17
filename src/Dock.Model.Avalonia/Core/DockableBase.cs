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
        public static readonly DirectProperty<DockableBase, string> IdProperty =
            AvaloniaProperty.RegisterDirect<DockableBase, string>(nameof(Id), o => o.Id, (o, v) => o.Id = v);

        /// <summary>
        /// Defines the <see cref="Title"/> property.
        /// </summary>
        public static readonly DirectProperty<DockableBase, string> TitleProperty =
            AvaloniaProperty.RegisterDirect<DockableBase, string>(nameof(Title), o => o.Title, (o, v) => o.Title = v);

        /// <summary>
        /// Defines the <see cref="Context"/> property.
        /// </summary>
        public static readonly DirectProperty<DockableBase, object> ContextProperty =
            AvaloniaProperty.RegisterDirect<DockableBase, object>(nameof(Context), o => o.Context, (o, v) => o.Context = v);

        /// <summary>
        /// Defines the <see cref="Owner"/> property.
        /// </summary>
        public static readonly DirectProperty<DockableBase, IDockable> OwnerProperty =
            AvaloniaProperty.RegisterDirect<DockableBase, IDockable>(nameof(Owner), o => o.Owner, (o, v) => o.Owner = v);

        private string _id;
        private string _title;
        private object _context;
        private IDockable _owner;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Id
        {
            get => _id;
            set => SetAndRaise(IdProperty, ref _id, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string Title
        {
            get => _title;
            set => SetAndRaise(TitleProperty, ref _title, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public object Context
        {
            get => _context;
            set => SetAndRaise(ContextProperty, ref _context, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockable Owner
        {
            get => _owner;
            set => SetAndRaise(OwnerProperty, ref _owner, value);
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
        public abstract IDockable Clone();
    }
}
