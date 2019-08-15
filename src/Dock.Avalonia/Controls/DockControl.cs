// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DockControl"/> xaml.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        /// <summary>
        /// Defines the IsDragArea attached property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsDragAreaProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDragArea", false, false, BindingMode.TwoWay);

        /// <summary>
        /// Defines the IsDropArea attached property.
        /// </summary>
        public static readonly AttachedProperty<bool> IsDropAreaProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDropArea", false, false, BindingMode.TwoWay);

        /// <summary>
        /// Define IsDragEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsDragEnabledProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDragEnabled", true, true, BindingMode.TwoWay);

        /// <summary>
        /// Define IsDropEnabled attached property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsDropEnabledProperty =
            AvaloniaProperty.RegisterAttached<DockControl, IControl, bool>("IsDropEnabled", true, true, BindingMode.TwoWay);

        /// <summary>
        /// Gets the value of the IsDragArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDragArea attached property.</returns>
        public static bool GetIsDragArea(IControl control)
        {
            return control.GetValue(IsDragAreaProperty);
        }

        /// <summary>
        /// Sets the value of the IsDragArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDragArea property.</param>
        public static void SetIsDragArea(IControl control, bool value)
        {
            control.SetValue(IsDragAreaProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDropArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDropArea attached property.</returns>
        public static bool GetIsDropArea(IControl control)
        {
            return control.GetValue(IsDropAreaProperty);
        }

        /// <summary>
        /// Sets the value of the IsDropArea attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDropArea property.</param>
        public static void SetIsDropArea(IControl control, bool value)
        {
            control.SetValue(IsDropAreaProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDragEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDragEnabled attached property.</returns>
        public static bool GetIsDragEnabled(IControl control)
        {
            return control.GetValue(IsDragEnabledProperty);
        }

        /// <summary>
        /// Sets the value of the IsDragEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDragEnabled property.</param>
        public static void SetIsDragEnabled(IControl control, bool value)
        {
            control.SetValue(IsDragEnabledProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsDropEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The IsDropEnabled attached property.</returns>
        public static bool GetIsDropEnabled(IControl control)
        {
            return control.GetValue(IsDropEnabledProperty);
        }

        /// <summary>
        /// Sets the value of the IsDropEnabled attached property on the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value of the IsDropEnabled property.</param>
        public static void SetIsDropEnabled(IControl control, bool value)
        {
            control.SetValue(IsDropEnabledProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Layout"/> property.
        /// </summary>
        public static readonly StyledProperty<IDock> LayoutProperty =
            AvaloniaProperty.Register<DockControl, IDock>(nameof(Layout));

        /// <summary>
        /// Gets or sets the dock layout.
        /// </summary>
        /// <value>The layout.</value>
        [Content]
        public IDock Layout
        {
            get { return GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        private void Process(IInteractive source, Point point, string name)
        {
            if (VisualRoot is IInputElement input)
            {
                var local = this.TranslatePoint(point, input);
                var controls = input.InputHitTest(local.Value)?.GetSelfAndVisualDescendants()?.OfType<IControl>().ToList();
                if (controls?.Count > 0)
                {
                    Debug.WriteLine($"{name} : {source.GetType().Name} : {local.Value}");
                    foreach (var control in controls)
                    {
                        if (Process(control, local.Value) == true)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static bool Process(IControl control, Point point)
        {
            bool result = false;

            //Debug.WriteLine($"- Process : {point} : {control.Name} : {control.GetType().Name} : {control.DataContext?.GetType().Name}");

            if (control.GetValue(DockControl.IsDropAreaProperty) == true)
            {
                //Debug.WriteLine($"- Drop : {point} : {control.Name} : {control.GetType().Name} : {control.DataContext?.GetType().Name}");
                result = true;
            }

            if (control.GetValue(DockControl.IsDragAreaProperty) == true)
            {
                Debug.WriteLine($"- Drag : {point} : {control.Name} : {control.GetType().Name} : {control.DataContext?.GetType().Name}");
                result = true;
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Process(e.Source, e.GetPosition(this), nameof(OnPointerPressed));
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            Process(e.Source, e.GetPosition(this), nameof(OnPointerReleased));
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            Process(e.Source, e.GetPosition(this), nameof(OnPointerMoved));
        }

        /// <inheritdoc/>
        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            //Process(e.Source, e.GetPosition(this), nameof(OnPointerEnter));
        }

        /// <inheritdoc/>
        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);
            //Process(e.Source, e.GetPosition(this), nameof(OnPointerLeave));
        }

        /// <inheritdoc/>
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            Process(e.Source, e.GetPosition(this), nameof(OnPointerWheelChanged));
        }
    }
}
