// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dock.Model
{
    /// <summary>
    /// Observable object base class.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify observers about property changes.
        /// </summary>
        /// <param name="propertyName">The property name that changed.</param>
        public void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Update property backing field and notify observers about property change.
        /// </summary>
        /// <typeparam name="T">The type of field.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="value">The new field value.</param>
        /// <param name="propertyName">The property name that changed.</param>
        /// <returns>True if backing field value changed.</returns>
        public bool Update<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                Notify(propertyName);
                return true;
            }
            return false;
        }
    }
}
