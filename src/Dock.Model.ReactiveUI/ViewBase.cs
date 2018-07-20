// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using ReactiveUI;

namespace Dock.Model
{
    /// <summary>
    /// View base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class ViewBase : ReactiveObject, IView
    {
        private string _id;
        private string _title;
        private object _context;
        private IView _parent;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
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
        public object Context
        {
            get => _context;
            set => this.RaiseAndSetIfChanged(ref _context, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IView Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        /// <inheritdoc/>
        public virtual bool OnClose()
        {
            return true;
        }
    }
}
