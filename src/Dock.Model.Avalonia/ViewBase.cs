// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model
{
    /// <summary>
    /// View base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class ViewBase : AvaloniaObject, IView
    {
        /// <summary>
        /// Defines the <see cref="Id"/> property.
        /// </summary>
        public static readonly StyledProperty<string> IdProperty = 
            AvaloniaProperty.Register<ViewBase, string>(nameof(IdProperty));

        /// <summary>
        /// Defines the <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<ViewBase, string>(nameof(TitleProperty));

        /// <summary>
        /// Defines the <see cref="Context"/> property.
        /// </summary>
        public static readonly StyledProperty<object> ContextProperty =
            AvaloniaProperty.Register<ViewBase, object>(nameof(ContextProperty));

        /// <summary>
        /// Defines the <see cref="Parent"/> property.
        /// </summary>
        public static readonly StyledProperty<IView> ParentProperty =
            AvaloniaProperty.Register<ViewBase, IView>(nameof(ParentProperty));

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
        [Content]
        [IgnoreDataMember]
        public object Context
        {
            get { return GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IView Parent
        {
            get { return GetValue(ParentProperty); }
            set { SetValue(ParentProperty, value); }
        }

        static ViewBase()
        {
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
