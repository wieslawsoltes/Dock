// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Root dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class RootDock : DockBase, IRootDock
    {
        private IDockWindow _window;
        private IPinDock _top;
        private IPinDock _bottom;
        private IPinDock _left;
        private IPinDock _right;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockWindow Window
        {
            get => _window;
            set => this.RaiseAndSetIfChanged(ref _window, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Top
        {
            get => _top;
            set => this.RaiseAndSetIfChanged(ref _top, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Bottom
        {
            get => _bottom;
            set => this.RaiseAndSetIfChanged(ref _bottom, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Left
        {
            get => _left;
            set => this.RaiseAndSetIfChanged(ref _left, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Right
        {
            get => _right;
            set => this.RaiseAndSetIfChanged(ref _right, value);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneRootDock(this);
        }
    }
}
