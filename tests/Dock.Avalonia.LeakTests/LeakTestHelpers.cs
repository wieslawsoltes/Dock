using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using AvaloniaPointer = Avalonia.Input.Pointer;

namespace Dock.Avalonia.LeakTests;

internal static class LeakTestHelpers
{
    internal static bool IsLeakTraceEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DOCK_LEAK_TRACE"),
        "1",
        StringComparison.Ordinal);
    [Flags]
    internal enum InputInteractionMask
    {
        None = 0,
        PointerEnterExit = 1 << 0,
        PointerMove = 1 << 1,
        PointerPressRelease = 1 << 2,
        PointerWheel = 1 << 3,
        RightClick = 1 << 4,
        MiddleClick = 1 << 5,
        KeyPress = 1 << 6,
        TextInput = 1 << 7,
        CaptureLost = 1 << 8,
        DoubleTap = 1 << 9,
        All = PointerEnterExit
              | PointerMove
              | PointerPressRelease
              | PointerWheel
              | RightClick
              | KeyPress
              | TextInput
              | CaptureLost
    }
    internal static void AssertCollected(params WeakReference[] references)
    {
        for (var attempt = 0; attempt < 20 && references.Any(reference => reference.IsAlive); attempt++)
        {
            CollectGarbage();
            Thread.Sleep(10);
        }

        foreach (var reference in references)
        {
            Assert.False(reference.IsAlive);
        }
    }

    private static void ScrubStaticReferences(object target)
    {
        var targetType = target.GetType();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName is null)
            {
                continue;
            }

            if (!assembly.FullName.StartsWith("Avalonia", StringComparison.Ordinal)
                && !assembly.FullName.StartsWith("Dock.", StringComparison.Ordinal))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type is not null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type is null)
                {
                    continue;
                }

                FieldInfo[] fields;
                try
                {
                    fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                }
                catch
                {
                    continue;
                }

                foreach (var field in fields)
                {
                    if (field.IsInitOnly || field.FieldType.IsValueType)
                    {
                        continue;
                    }

                    object? value;
                    try
                    {
                        value = field.GetValue(null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (value is null)
                    {
                        continue;
                    }

                    if (ReferenceEquals(value, target))
                    {
                        TrySetField(field, null);
                        continue;
                    }

                    if (value is IDictionary dictionary)
                    {
                        RemoveFromDictionary(dictionary, target);
                        continue;
                    }

                    if (value is IList list)
                    {
                        RemoveFromList(list, target);
                        continue;
                    }

                    var remove = value.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { targetType }, null)
                                 ?? value.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object) }, null);
                    if (remove is not null)
                    {
                        try
                        {
                            remove.Invoke(value, new object?[] { target });
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    private static void RemoveFromDictionary(IDictionary dictionary, object target)
    {
        var keysToRemove = new List<object?>();
        foreach (DictionaryEntry entry in dictionary)
        {
            if (ReferenceEquals(entry.Key, target) || ReferenceEquals(entry.Value, target)
                || IsWeakReferenceTo(entry.Key, target) || IsWeakReferenceTo(entry.Value, target))
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (key is null)
            {
                continue;
            }

            try
            {
                dictionary.Remove(key);
            }
            catch
            {
            }
        }
    }

    private static void RemoveFromList(IList list, object target)
    {
        for (var index = list.Count - 1; index >= 0; index--)
        {
            object? item;
            try
            {
                item = list[index];
            }
            catch
            {
                continue;
            }

            if (ReferenceEquals(item, target) || IsWeakReferenceTo(item, target))
            {
                try
                {
                    list.RemoveAt(index);
                }
                catch
                {
                }
            }
        }
    }

    private static bool IsWeakReferenceTo(object? candidate, object target)
    {
        if (candidate is WeakReference weakReference)
        {
            return ReferenceEquals(weakReference.Target, target);
        }

        var candidateType = candidate?.GetType();
        if (candidateType is null || !candidateType.IsGenericType)
        {
            return false;
        }

        if (candidateType.GetGenericTypeDefinition() != typeof(WeakReference<>))
        {
            return false;
        }

        var tryGetTarget = candidateType.GetMethod("TryGetTarget", BindingFlags.Instance | BindingFlags.Public);
        if (tryGetTarget is null)
        {
            return false;
        }

        var args = new object?[] { null };
        var success = (bool)tryGetTarget.Invoke(candidate, args)!;
        return success && ReferenceEquals(args[0], target);
    }

    private static void TrySetField(FieldInfo field, object? value)
    {
        try
        {
            field.SetValue(null, value);
        }
        catch
        {
        }
    }

    internal static void CollectGarbage()
    {
        DrainDispatcher();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        DrainDispatcher();
    }

    internal static void DrainDispatcher()
    {
        var dispatcher = Dispatcher.UIThread;
        for (var i = 0; i < 5; i++)
        {
            dispatcher.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
    }

    internal static void EnsureFontManager()
    {
        if (GetAvaloniaService(typeof(IFontManagerImpl)) is not IFontManagerImpl fontManagerImpl)
        {
            return;
        }

        var locatorType = typeof(AvaloniaLocator);
        var currentMutable = locatorType.GetProperty("CurrentMutable", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(null);
        if (currentMutable is null)
        {
            return;
        }

        var registryField = currentMutable.GetType().GetField("_registry", BindingFlags.Instance | BindingFlags.NonPublic);
        if (registryField?.GetValue(currentMutable) is IDictionary registry)
        {
            registry[typeof(FontManager)] = (Func<object?>)(() => new FontManager(fontManagerImpl));
        }
    }

    internal static void ShowWindow(Window window)
    {
        if (window.SizeToContent != SizeToContent.Manual)
        {
            window.SizeToContent = SizeToContent.Manual;
        }

        if (double.IsNaN(window.Width) || window.Width <= 0)
        {
            window.Width = 800;
        }

        if (double.IsNaN(window.Height) || window.Height <= 0)
        {
            window.Height = 600;
        }

        if (window.Content is Control contentControl)
        {
            EnsureControlTheme(contentControl);
        }

        window.Show();
        DrainDispatcher();
        window.UpdateLayout();
        DrainDispatcher();
    }

    private static void EnsureControlTheme(Control control)
    {
        if (control is not TemplatedControl templated || templated.Theme is not null)
        {
            return;
        }

        if (control.TryFindResource(control.GetType(), out var resource) && resource is ControlTheme theme)
        {
            templated.Theme = theme;
        }
    }

    internal static void CleanupWindow(Window window)
    {
        window.FocusManager?.ClearFocus();
        ClearInputState(window);
        window.Content = null;
        window.DataContext = null;
        window.Close();
        DrainDispatcher();
        ForceHandleClosed(window);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
            && lifetime.Windows is IList<Window> windows)
        {
            foreach (var other in windows.OfType<PinnedDockWindow>().ToList())
            {
                if (other != window)
                {
                    other.Close();
                }
                windows.Remove(other);
            }

            windows.Remove(window);
        }
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime classicLifetime)
        {
            var windowsField = classicLifetime.GetType().GetField("_windows", BindingFlags.Instance | BindingFlags.NonPublic);
            if (windowsField?.GetValue(classicLifetime) is IList list)
            {
                list.Clear();
            }
        }
        ClearPlatformImpl(window);
        ClearLayoutState(window);
        ClearInputState(window);
    }


    internal static void ExerciseInputInteractions(
        Control control,
        bool includeDoubleTap = false,
        bool includeMiddlePress = false,
        InputInteractionMask? interactionMask = null,
        bool clearInputState = true)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out var point))
        {
            return;
        }

        var interactions = ResolveInteractionMask(interactionMask, includeDoubleTap, includeMiddlePress);

        var dockControl = control as DockControl;
        var previousDockingEnabled = dockControl?.IsDockingEnabled ?? true;
        if (dockControl is not null)
        {
            dockControl.IsDockingEnabled = false;
        }

        try
        {
            var topLevel = control as TopLevel ?? TopLevel.GetTopLevel(control);
            if (topLevel is not null)
            {
                var rootPoint = control.TranslatePoint(point, topLevel) ?? point;
                if (interactions.HasFlag(InputInteractionMask.PointerEnterExit))
                {
                    RaisePointerEntered(control, root, pointer, point);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerMove))
                {
                    topLevel.MouseMove(rootPoint, RawInputModifiers.None);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerPressRelease))
                {
                    topLevel.MouseDown(rootPoint, MouseButton.Left, RawInputModifiers.None);
                    if (interactions.HasFlag(InputInteractionMask.PointerMove))
                    {
                        topLevel.MouseMove(rootPoint, RawInputModifiers.LeftMouseButton);
                    }
                    topLevel.MouseUp(rootPoint, MouseButton.Left, RawInputModifiers.None);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerWheel))
                {
                    topLevel.MouseWheel(rootPoint, new Vector(0, 1), RawInputModifiers.None);
                }

                if (interactions.HasFlag(InputInteractionMask.RightClick))
                {
                    topLevel.MouseDown(rootPoint, MouseButton.Right, RawInputModifiers.RightMouseButton);
                    topLevel.MouseUp(rootPoint, MouseButton.Right, RawInputModifiers.None);
                }

                if (interactions.HasFlag(InputInteractionMask.MiddleClick))
                {
                    topLevel.MouseDown(rootPoint, MouseButton.Middle, RawInputModifiers.MiddleMouseButton);
                    topLevel.MouseUp(rootPoint, MouseButton.Middle, RawInputModifiers.None);
                }

                if (interactions.HasFlag(InputInteractionMask.KeyPress))
                {
                    control.Focus();
                    if (control.IsKeyboardFocusWithin)
                    {
                        topLevel.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.None);
                        topLevel.KeyReleaseQwerty(PhysicalKey.A, RawInputModifiers.None);
                    }
                }

                if (interactions.HasFlag(InputInteractionMask.TextInput))
                {
                    control.Focus();
                    if (control.IsKeyboardFocusWithin)
                    {
                        topLevel.KeyTextInput("x");
                    }
                }

                if (interactions.HasFlag(InputInteractionMask.CaptureLost))
                {
                    RaisePointerCaptureLost(control, pointer);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerEnterExit))
                {
                    RaisePointerExited(control, root, pointer, point);
                }
            }
            else
            {
                if (interactions.HasFlag(InputInteractionMask.PointerEnterExit))
                {
                    RaisePointerEntered(control, root, pointer, point);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerMove))
                {
                    RaisePointerMoved(control, root, pointer, point, leftPressed: false);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerPressRelease))
                {
                    RaisePointerPressed(control, root, pointer, point, MouseButton.Left);
                    if (interactions.HasFlag(InputInteractionMask.PointerMove))
                    {
                        RaisePointerMoved(control, root, pointer, point, leftPressed: true);
                    }
                    RaisePointerReleased(control, root, pointer, point, MouseButton.Left);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerWheel))
                {
                    RaisePointerWheel(control, root, pointer, point, new Vector(0, 1));
                }

                if (interactions.HasFlag(InputInteractionMask.RightClick))
                {
                    RaisePointerPressed(control, root, pointer, point, MouseButton.Right);
                    RaisePointerReleased(control, root, pointer, point, MouseButton.Right);
                }

                if (interactions.HasFlag(InputInteractionMask.MiddleClick))
                {
                    RaisePointerPressed(control, root, pointer, point, MouseButton.Middle);
                    RaisePointerReleased(control, root, pointer, point, MouseButton.Middle);
                }

                if (interactions.HasFlag(InputInteractionMask.CaptureLost))
                {
                    RaisePointerCaptureLost(control, pointer);
                }

                if (interactions.HasFlag(InputInteractionMask.PointerEnterExit))
                {
                    RaisePointerExited(control, root, pointer, point);
                }

                // Skip key/text input when there is no input root to route through.
            }

            if (interactions.HasFlag(InputInteractionMask.DoubleTap))
            {
                RaiseDoubleTapped(control, root, pointer, point);
            }
        }
        finally
        {
            pointer.Capture(null!);
            if (dockControl is not null)
            {
                dockControl.IsDockingEnabled = previousDockingEnabled;
            }
        }

        if (clearInputState)
        {
            DrainDispatcher();
            ClearInputState(control);
            DrainDispatcher();
        }
    }

    internal static void OpenAndCloseContextMenu(Control target, ContextMenu? menu)
    {
        if (menu is null)
        {
            return;
        }

        var placementTarget = FindContextMenuOwner(target, menu)
                              ?? (ReferenceEquals(target.ContextMenu, menu) ? target : null)
                              ?? menu.PlacementTarget as Control;
        if (placementTarget is null)
        {
            return;
        }

        menu.PlacementTarget = placementTarget;
        menu.Open(placementTarget);
        DrainDispatcher();
        menu.Close();
        DrainDispatcher();
    }

    private static Control? FindContextMenuOwner(Control target, ContextMenu menu)
    {
        if (ReferenceEquals(target.ContextMenu, menu))
        {
            return target;
        }

        foreach (var visual in target.GetVisualDescendants())
        {
            if (visual is Control control && ReferenceEquals(control.ContextMenu, menu))
            {
                return control;
            }
        }

        return null;
    }

    internal static void OpenAndCloseFlyout(Control target, FlyoutBase? flyout)
    {
        if (flyout is null)
        {
            return;
        }

        flyout.ShowAt(target);
        DrainDispatcher();
        flyout.Hide();
        DrainDispatcher();
    }

    internal static T? FindVisualDescendant<T>(Visual root, Func<T, bool>? predicate = null) where T : Visual
    {
        if (root is T match && (predicate is null || predicate(match)))
        {
            return match;
        }

        foreach (var visual in root.GetVisualDescendants())
        {
            if (visual is T typed && (predicate is null || predicate(typed)))
            {
                return typed;
            }
        }

        return null;
    }

    private static InputInteractionMask ResolveInteractionMask(
        InputInteractionMask? interactionMask,
        bool includeDoubleTap,
        bool includeMiddlePress)
    {
        var interactions = interactionMask ?? InputInteractionMask.All;

        if (includeDoubleTap)
        {
            interactions |= InputInteractionMask.DoubleTap;
        }

        if (includeMiddlePress)
        {
            interactions |= InputInteractionMask.MiddleClick;
        }

        var overrideValue = Environment.GetEnvironmentVariable("DOCK_LEAK_INTERACTIONS");
        if (string.IsNullOrWhiteSpace(overrideValue))
        {
            return interactions;
        }

        if (int.TryParse(overrideValue, out var numericMask))
        {
            return (InputInteractionMask)numericMask;
        }

        var resolved = InputInteractionMask.None;
        var anyParsed = false;
        var parts = overrideValue.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (Enum.TryParse<InputInteractionMask>(part, ignoreCase: true, out var flag))
            {
                resolved |= flag;
                anyParsed = true;
            }
        }

        return anyParsed ? resolved : interactions;
    }

    internal static void RaisePointerPressed(Control control, MouseButton button, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out var point))
        {
            return;
        }

        RaisePointerPressed(control, root, pointer, point, button, modifiers);
    }

    internal static void RaisePointerPressed(Control control, Point point, MouseButton button, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out _))
        {
            return;
        }

        RaisePointerPressed(control, root, pointer, point, button, modifiers);
    }

    internal static void RaisePointerMoved(Control control, Point point, bool leftPressed)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out _))
        {
            return;
        }

        RaisePointerMoved(control, root, pointer, point, leftPressed);
    }

    internal static void RaisePointerReleased(Control control, Point point, MouseButton button)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out _))
        {
            return;
        }

        RaisePointerReleased(control, root, pointer, point, button);
    }

    internal static void RaiseDoubleTapped(Control control)
    {
        if (!TryGetInputContext(control, out var root, out var pointer, out var point))
        {
            return;
        }

        RaiseDoubleTapped(control, root, pointer, point);
    }

    private static void RaisePointerPressed(Control control, Visual root, AvaloniaPointer pointer, Point point, MouseButton button, KeyModifiers modifiers = KeyModifiers.None)
    {
        var (rawModifiers, updateKind) = GetPointerUpdate(button, pressed: true);
        var properties = new PointerPointProperties(rawModifiers, updateKind);
        var args = new PointerPressedEventArgs(control, pointer, root, point, 0, properties, modifiers, 1);
        control.RaiseEvent(args);
    }

    private static void RaisePointerMoved(Control control, Visual root, AvaloniaPointer pointer, Point point, bool leftPressed)
    {
        var rawModifiers = leftPressed ? RawInputModifiers.LeftMouseButton : RawInputModifiers.None;
        var properties = new PointerPointProperties(rawModifiers, PointerUpdateKind.Other);
        var args = new PointerEventArgs(InputElement.PointerMovedEvent, control, pointer, root, point, 0, properties, KeyModifiers.None);
        control.RaiseEvent(args);
    }

    private static void RaisePointerEntered(Control control, Visual root, AvaloniaPointer pointer, Point point)
    {
        var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
        var args = new PointerEventArgs(InputElement.PointerEnteredEvent, control, pointer, root, point, 0, properties, KeyModifiers.None);
        control.RaiseEvent(args);
    }

    private static void RaisePointerExited(Control control, Visual root, AvaloniaPointer pointer, Point point)
    {
        var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
        var args = new PointerEventArgs(InputElement.PointerExitedEvent, control, pointer, root, point, 0, properties, KeyModifiers.None);
        control.RaiseEvent(args);
    }

    private static void RaisePointerReleased(Control control, Visual root, AvaloniaPointer pointer, Point point, MouseButton button)
    {
        var (_, updateKind) = GetPointerUpdate(button, pressed: false);
        var properties = new PointerPointProperties(RawInputModifiers.None, updateKind);
        var args = new PointerReleasedEventArgs(control, pointer, root, point, 0, properties, KeyModifiers.None, button);
        control.RaiseEvent(args);
    }

    private static void RaisePointerWheel(Control control, Visual root, AvaloniaPointer pointer, Point point, Vector delta)
    {
        var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
        var args = new PointerWheelEventArgs(control, pointer, root, point, 0, properties, KeyModifiers.None, delta);
        control.RaiseEvent(args);
    }

    private static void RaisePointerCaptureLost(Control control, AvaloniaPointer pointer)
    {
        var args = new PointerCaptureLostEventArgs(control, pointer);
        control.RaiseEvent(args);
    }

    private static void RaiseDoubleTapped(Control control, Visual root, AvaloniaPointer pointer, Point point)
    {
        var properties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
        var pressedArgs = new PointerPressedEventArgs(control, pointer, root, point, 0, properties, KeyModifiers.None, 2);
        var tappedArgs = new TappedEventArgs(InputElement.DoubleTappedEvent, pressedArgs);
        control.RaiseEvent(tappedArgs);
    }

    private static (RawInputModifiers RawModifiers, PointerUpdateKind UpdateKind) GetPointerUpdate(MouseButton button, bool pressed)
    {
        return button switch
        {
            MouseButton.Left => pressed
                ? (RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed)
                : (RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            MouseButton.Middle => pressed
                ? (RawInputModifiers.MiddleMouseButton, PointerUpdateKind.MiddleButtonPressed)
                : (RawInputModifiers.None, PointerUpdateKind.MiddleButtonReleased),
            MouseButton.Right => pressed
                ? (RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed)
                : (RawInputModifiers.None, PointerUpdateKind.RightButtonReleased),
            _ => (RawInputModifiers.None, PointerUpdateKind.Other)
        };
    }

    private static bool TryGetInputContext(Control control, out Visual root, out AvaloniaPointer pointer, out Point point)
    {
        root = control;
        pointer = new AvaloniaPointer(1, PointerType.Mouse, true);
        point = GetSafePoint(control);
        return true;
    }

    private static Point GetSafePoint(Control control)
    {
        var bounds = control.Bounds;
        var width = double.IsFinite(bounds.Width) ? bounds.Width : 0;
        var height = double.IsFinite(bounds.Height) ? bounds.Height : 0;
        var x = width > 1 ? width / 2 : 1;
        var y = height > 1 ? height / 2 : 1;
        return new Point(x, y);
    }

    internal static void ClearInputState(Control? control)
    {
        var topLevel = control as TopLevel ?? (control is not null ? TopLevel.GetTopLevel(control) : null);
        ClearInputState(topLevel);
    }

    internal static void ClearInputState(TopLevel? topLevel)
    {
        if (topLevel is not null)
        {
            ClearPointerOverPreProcessor(topLevel);
            ClearTopLevelHandlers(topLevel);
            topLevel.ClearValue(TopLevel.PointerOverElementProperty);
            ClearInputManagerPointerState(topLevel);
            ClearInputManagerObservers(topLevel);
            ScrubInputManagerInstance(topLevel);
        }

        ClearKeyboardDeviceState();
        ClearMouseDeviceState(topLevel);
        ClearStaticInputReferences();
        ClearWeakEventSubscriptions();
        ClearDockStaticState();
        ClearRenderState();
        ClearDispatcherState();
        ScrubInputReferences();
    }

    private static void ClearLayoutState(TopLevel? topLevel)
    {
        if (topLevel is null)
        {
            return;
        }

        var layoutManagerField = typeof(TopLevel).GetField("_layoutManager", BindingFlags.Instance | BindingFlags.NonPublic);
        if (layoutManagerField?.GetValue(topLevel) is not object layoutManager)
        {
            return;
        }

        if (layoutManager is IDisposable disposable)
        {
            disposable.Dispose();
        }

        ClearCollectionField(layoutManager, "_toMeasure");
        ClearCollectionField(layoutManager, "_toArrange");
        ClearCollectionField(layoutManager, "_toArrangeAfterMeasure");
        ClearCollectionField(layoutManager, "_effectiveViewportChangedListeners");

        layoutManagerField.SetValue(topLevel, null);
    }

    private static void ForceHandleClosed(TopLevel topLevel)
    {
        var handleClosed = typeof(TopLevel).GetMethod("HandleClosed", BindingFlags.Instance | BindingFlags.NonPublic);
        handleClosed?.Invoke(topLevel, null);
    }

    private static void ClearCollectionField(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            return;
        }

        var value = field.GetValue(target);
        switch (value)
        {
            case null:
                return;
            case System.Collections.IDictionary dictionary:
                dictionary.Clear();
                return;
            case IList list:
                list.Clear();
                return;
            default:
                var clearMethod = value.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                clearMethod?.Invoke(value, null);
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
        }
    }

    internal static void ClearPinnedDockControlState(PinnedDockControl? control)
    {
        if (control is null)
        {
            return;
        }

        if (control.DataContext is IRootDock root)
        {
            root.PinnedDock = null;
            root.LeftPinnedDockables?.Clear();
            root.RightPinnedDockables?.Clear();
            root.TopPinnedDockables?.Clear();
            root.BottomPinnedDockables?.Clear();
            ClearFactoryCaches(root.Factory);
        }

        var controlType = typeof(PinnedDockControl);
        controlType.GetMethod("CloseWindow", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(control, null);
        controlType.GetMethod("ClearPinnedDockableSubscription", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(control, null);

        SetField(control, "_managedLayer", null);
        SetField(control, "_managedPinnedHost", null);
        SetField(control, "_ownerWindow", null);
        SetField(control, "_window", null);
        SetField(control, "_trackedDockable", null);
        SetField(control, "_trackedAvaloniaDockable", null);
        SetField(control, "_trackedNotifyDockable", null);
        SetField(control, "_pinnedDock", null);
        SetField(control, "_pinnedDockGrid", null);
        SetField(control, "_pinnedDockSplitter", null);
    }

    internal static void ClearFactoryCaches(IFactory? factory)
    {
        if (factory is null)
        {
            return;
        }

        factory.VisibleDockableControls.Clear();
        factory.VisibleRootControls.Clear();
        factory.PinnedDockableControls.Clear();
        factory.PinnedRootControls.Clear();
        factory.TabDockableControls.Clear();
        factory.TabRootControls.Clear();
        factory.ToolControls.Clear();
        factory.DocumentControls.Clear();
        factory.DockControls.Clear();
        factory.HostWindows.Clear();
    }

    private static void ClearPointerOverPreProcessor(TopLevel topLevel)
    {
        var field = typeof(TopLevel).GetField("_pointerOverPreProcessor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(topLevel) is not object preProcessor)
        {
            return;
        }

        var clearMethod = preProcessor.GetType().GetMethod(
            "ClearPointerOver",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            Type.EmptyTypes);

        if (clearMethod is not null)
        {
            clearMethod.Invoke(preProcessor, null);
        }

        SetField(preProcessor, "_inputRoot", null);
        SetField(preProcessor, "_currentPointer", null);
        SetField(preProcessor, "_lastActivePointerDevice", null);
        SetField(preProcessor, "_lastKnownPosition", null);

        var subscriptionField = typeof(TopLevel).GetField("_pointerOverPreProcessorSubscription", BindingFlags.Instance | BindingFlags.NonPublic);
        if (subscriptionField?.GetValue(topLevel) is IDisposable subscription)
        {
            subscription.Dispose();
            subscriptionField.SetValue(topLevel, null);
        }

        field.SetValue(topLevel, null);
    }

    private static void ClearTopLevelHandlers(TopLevel topLevel)
    {
        var focusManager = topLevel.FocusManager;
        if (focusManager is not null)
        {
            SetField(focusManager, "_focusRoot", null);
        }

        var focusedElementProperty = typeof(FocusManager).GetField("FocusedElementProperty", BindingFlags.Static | BindingFlags.NonPublic);
        if (focusedElementProperty?.GetValue(null) is AvaloniaProperty property)
        {
            topLevel.ClearValue(property);
        }

        ClearHandlerOwner(topLevel, "_keyboardNavigationHandler", "_owner");
        ClearHandlerOwner(topLevel, "_accessKeyHandler", "_owner");
        ClearHandlerOwner(topLevel, "_accessKeyHandler", "_restoreFocusElementRef");
        ClearHandlerRegistrations(topLevel, "_accessKeyHandler", "_registrations");
    }

    private static void ClearInputManagerPointerState(TopLevel topLevel)
    {
        var inputManagerField = typeof(TopLevel).GetField("_inputManager", BindingFlags.Instance | BindingFlags.NonPublic);
        var inputManager = inputManagerField?.GetValue(topLevel);
        if (inputManager is null)
        {
            return;
        }

        if (GetStaticMemberValue(typeof(MouseDevice), "Primary", "_primary") is not IInputDevice device)
        {
            return;
        }

        var position = new Point(0, 0);
        var leaveArgs = CreateRawPointerEventArgs(device, topLevel, RawPointerEventType.LeaveWindow, position);
        if (leaveArgs is not null)
        {
            InvokeInputManager(inputManager, leaveArgs);
        }

        var cancelArgs = CreateRawPointerEventArgs(device, topLevel, RawPointerEventType.CancelCapture, position);
        if (cancelArgs is not null)
        {
            InvokeInputManager(inputManager, cancelArgs);
        }
    }

    private static void ClearInputManagerObservers(TopLevel topLevel)
    {
        var inputManagerField = typeof(TopLevel).GetField("_inputManager", BindingFlags.Instance | BindingFlags.NonPublic);
        var inputManager = inputManagerField?.GetValue(topLevel);
        if (inputManager is null)
        {
            return;
        }

        foreach (var fieldName in new[] { "_preProcess", "_process", "_postProcess" })
        {
            var subjectField = inputManager.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (subjectField?.GetValue(inputManager) is not object subject)
            {
                continue;
            }

            var baseType = subject.GetType().BaseType;
            var observersField = baseType?.GetField("_observers", BindingFlags.Instance | BindingFlags.NonPublic);
            if (observersField?.GetValue(subject) is System.Collections.IList observers)
            {
                observers.Clear();
            }
        }
    }

    private static void ScrubInputManagerInstance(TopLevel topLevel)
    {
        var inputManagerField = typeof(TopLevel).GetField("_inputManager", BindingFlags.Instance | BindingFlags.NonPublic);
        var inputManager = inputManagerField?.GetValue(topLevel);
        if (inputManager is null)
        {
            return;
        }

        var inputRootType = typeof(IInputRoot);
        var inputElementType = typeof(IInputElement);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        ScrubObject(inputManager, inputRootType, inputElementType, visited, depth: 0);
    }

    private static void InvokeInputManager(object inputManager, RawInputEventArgs args)
    {
        var method = inputManager.GetType().GetMethod(
            "ProcessInput",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(RawInputEventArgs) },
            null);

        method?.Invoke(inputManager, new object?[] { args });
    }

    private static RawPointerEventArgs? CreateRawPointerEventArgs(IInputDevice device, IInputRoot root, RawPointerEventType type, Point position)
    {
        var args = new object?[]
        {
            device,
            0ul,
            root,
            type,
            position,
            RawInputModifiers.None
        };

        return Activator.CreateInstance(typeof(RawPointerEventArgs), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null)
            as RawPointerEventArgs;
    }

    private static void ClearKeyboardDeviceState()
    {
        var keyboard = GetAvaloniaService(typeof(IKeyboardDevice))
            ?? GetStaticMemberValue(typeof(KeyboardDevice), "Instance");
        if (keyboard is null)
        {
            return;
        }

        ClearTextInputManagerState(keyboard);
        SetField(keyboard, "_focusedElement", null);
        SetField(keyboard, "_focusedRoot", null);
    }

    private static void ClearTextInputManagerState(object keyboard)
    {
        var managerField = keyboard.GetType().GetField("_textInputManager", BindingFlags.Instance | BindingFlags.NonPublic);
        if (managerField?.GetValue(keyboard) is not object manager)
        {
            return;
        }

        var setFocusedElement = manager.GetType().GetMethod(
            "SetFocusedElement",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(IInputElement) },
            null);

        setFocusedElement?.Invoke(manager, new object?[] { null });

        var visualRootField = manager.GetType().GetField("_visualRoot", BindingFlags.Instance | BindingFlags.NonPublic);
        var visualRoot = visualRootField?.GetValue(manager);
        if (visualRoot is not null)
        {
            var removeHandler = typeof(InputMethod).GetMethod(
                "RemoveTextInputMethodClientRequeryRequestedHandler",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(Interactive), typeof(EventHandler<RoutedEventArgs>) },
                null);

            var handlerMethod = manager.GetType().GetMethod(
                "TextInputMethodClientRequeryRequested",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (removeHandler is not null && handlerMethod is not null)
            {
                var handler = Delegate.CreateDelegate(typeof(EventHandler<RoutedEventArgs>), manager, handlerMethod, throwOnBindFailure: false);
                if (handler is not null)
                {
                    removeHandler.Invoke(null, new object?[] { visualRoot, handler });
                }
            }
        }

        SetField(manager, "_focusedElement", null);
        SetField(manager, "_visualRoot", null);
        SetField(manager, "_client", null);
        SetField(manager, "_im", null);
    }

    private static void ClearMouseDeviceState(TopLevel? topLevel)
    {
        var mouse = GetTopLevelMouseDevice(topLevel)
            ?? GetAvaloniaService(typeof(IMouseDevice))
            ?? GetStaticMemberValue(typeof(MouseDevice), "Primary", "_primary");
        if (mouse is null)
        {
            return;
        }

        var pointerField = mouse.GetType().GetField("_pointer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pointerField?.GetValue(mouse) is not AvaloniaPointer pointer)
        {
            return;
        }

        pointer.Capture(null!);
        SetField(pointer, "<Captured>k__BackingField", null);
        SetField(pointer, "<CapturedGestureRecognizer>k__BackingField", null);
        SetField(pointer, "<IsGestureRecognitionSkipped>k__BackingField", false);
    }

    private static void SetField(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }

    private static void ClearHandlerOwner(TopLevel topLevel, string handlerFieldName, string ownerFieldName)
    {
        var handlerField = typeof(TopLevel).GetField(handlerFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (handlerField?.GetValue(topLevel) is not object handler)
        {
            return;
        }

        SetField(handler, ownerFieldName, null);
    }

    private static void ClearHandlerRegistrations(TopLevel topLevel, string handlerFieldName, string registrationsFieldName)
    {
        var handlerField = typeof(TopLevel).GetField(handlerFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (handlerField?.GetValue(topLevel) is not object handler)
        {
            return;
        }

        var registrationsField = handler.GetType().GetField(registrationsFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (registrationsField?.GetValue(handler) is System.Collections.IList registrations)
        {
            registrations.Clear();
        }
    }

    private static void ClearStaticInputReferences()
    {
        var inputRootType = typeof(IInputRoot);
        var inputElementType = typeof(IInputElement);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName is null || !assembly.FullName.StartsWith("Avalonia", StringComparison.Ordinal))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type is not null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type is null)
                {
                    continue;
                }

                if (IsLeakTraceEnabled && type.FullName == "Avalonia.Utilities.WeakEvents.AvaloniaPropertyChanged")
                {
                    Trace($"[LeakTrace:WeakEvents] Found AvaloniaPropertyChanged in {assembly.FullName}.");
                }

                FieldInfo[] fields;
                try
                {
                    fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                }
                catch
                {
                    continue;
                }

                foreach (var field in fields)
                {
                    if (field.IsInitOnly)
                    {
                        continue;
                    }

                    if (!inputRootType.IsAssignableFrom(field.FieldType) && !inputElementType.IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    if (field.GetValue(null) is null)
                    {
                        continue;
                    }

                    field.SetValue(null, null);
                }
            }
        }
    }

    private static void ClearWeakEventSubscriptions()
    {
        if (IsLeakTraceEnabled)
        {
            Trace("[LeakTrace:WeakEvents] Clearing weak event subscriptions.");
        }

        var avaloniaAssemblyName = typeof(AvaloniaObject).Assembly.FullName;
        Type? type = null;
        if (!string.IsNullOrWhiteSpace(avaloniaAssemblyName))
        {
            type = Type.GetType(
                $"Avalonia.Utilities.WeakEvents.AvaloniaPropertyChanged, {avaloniaAssemblyName}",
                throwOnError: false);
        }

        type ??= typeof(AvaloniaObject).Assembly
            .GetType("Avalonia.Utilities.WeakEvents.AvaloniaPropertyChanged", throwOnError: false);

        if (type is null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(found => found is not null).ToArray()!;
                }

                foreach (var candidate in types)
                {
                    if (candidate?.FullName == "Avalonia.Utilities.WeakEvents.AvaloniaPropertyChanged")
                    {
                        type = candidate;
                        break;
                    }
                }

                if (type is not null)
                {
                    break;
                }
            }
        }

        if (type is null)
        {
            if (IsLeakTraceEnabled)
            {
                Trace("[LeakTrace:WeakEvents] AvaloniaPropertyChanged type not found. Scanning weak event fields...");
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName is null || !assembly.FullName.StartsWith("Avalonia", StringComparison.Ordinal))
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(found => found is not null).ToArray()!;
                }

                foreach (var candidate in types)
                {
                    var subscriptionsField = candidate.GetField("_subscriptions", BindingFlags.Static | BindingFlags.NonPublic);
                    if (subscriptionsField is null)
                    {
                        continue;
                    }

                    var subscriptionsValue = subscriptionsField.GetValue(null);
                    if (subscriptionsValue is null)
                    {
                        continue;
                    }

                    if (IsLeakTraceEnabled)
                    {
                        Trace($"[LeakTrace:WeakEvents] Clearing {candidate.FullName}._subscriptions ({subscriptionsValue.GetType().FullName}).");
                    }

                    if (subscriptionsValue is IDictionary dictionaryValue)
                    {
                        dictionaryValue.Clear();
                        continue;
                    }

                    if (subscriptionsValue is IList listValue)
                    {
                        listValue.Clear();
                        continue;
                    }

                    var clearMethod = subscriptionsValue.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    clearMethod?.Invoke(subscriptionsValue, null);
                }
            }

            return;
        }

        var field = type.GetField("_subscriptions", BindingFlags.Static | BindingFlags.NonPublic);
        var subscriptions = field?.GetValue(null);
        if (subscriptions is null)
        {
            return;
        }

        if (IsLeakTraceEnabled)
        {
            Trace($"[LeakTrace:WeakEvents] SubscriptionsType={subscriptions.GetType().FullName}");
        }

        if (subscriptions is IDictionary dictionary)
        {
            dictionary.Clear();
            if (IsLeakTraceEnabled)
            {
                Trace("[LeakTrace:WeakEvents] Cleared IDictionary subscriptions.");
            }
            return;
        }

        if (subscriptions is IList list)
        {
            list.Clear();
            if (IsLeakTraceEnabled)
            {
                Trace("[LeakTrace:WeakEvents] Cleared IList subscriptions.");
            }
            return;
        }

        var clear = subscriptions.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (clear is not null)
        {
            clear.Invoke(subscriptions, null);
            if (IsLeakTraceEnabled)
            {
                Trace("[LeakTrace:WeakEvents] Cleared subscriptions via method.");
            }
        }
    }

    private static void ClearDockStaticState()
    {
        ClearDragPreviewHelper();
        ClearDragPreviewContext();
    }

    private static void ClearRenderState()
    {
        var mediaContextType = Type.GetType("Avalonia.Media.MediaContext, Avalonia.Base");
        if (mediaContextType is null)
        {
            return;
        }

        var mediaContext = GetAvaloniaService(mediaContextType)
            ?? mediaContextType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(null);

        if (mediaContext is null)
        {
            return;
        }

        var topLevelsField = mediaContextType.GetField("_topLevels", BindingFlags.Instance | BindingFlags.NonPublic);
        if (topLevelsField?.GetValue(mediaContext) is System.Collections.IDictionary topLevels)
        {
            topLevels.Clear();
        }

        var pendingBatchesField = mediaContextType.GetField("_pendingCompositionBatches", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pendingBatchesField?.GetValue(mediaContext) is System.Collections.IDictionary pendingBatches)
        {
            pendingBatches.Clear();
        }

        var requestedCommitsField = mediaContextType.GetField("_requestedCommits", BindingFlags.Instance | BindingFlags.NonPublic);
        if (requestedCommitsField?.GetValue(mediaContext) is object requestedCommits)
        {
            if (requestedCommits is IList list)
            {
                list.Clear();
            }
            else
            {
                requestedCommits.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.Invoke(requestedCommits, null);
            }
        }

        var invokeCallbacksField = mediaContextType.GetField("_invokeOnRenderCallbacks", BindingFlags.Instance | BindingFlags.NonPublic);
        if (invokeCallbacksField?.GetValue(mediaContext) is IList callbacks)
        {
            callbacks.Clear();
        }

        var callbackPoolField = mediaContextType.GetField("_invokeOnRenderCallbackListPool", BindingFlags.Instance | BindingFlags.NonPublic);
        if (callbackPoolField?.GetValue(mediaContext) is object pool)
        {
            if (pool is IList list)
            {
                list.Clear();
            }
            else
            {
                pool.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.Invoke(pool, null);
            }
        }

        var animationsTimerField = mediaContextType.GetField("_animationsTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (animationsTimerField?.GetValue(mediaContext) is object animationsTimer)
        {
            var stopMethod = animationsTimer.GetType().GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            stopMethod?.Invoke(animationsTimer, null);

            var tickField = animationsTimer.GetType().GetField("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
            tickField?.SetValue(animationsTimer, null);
        }
    }

    private static void ClearDispatcherState()
    {
        var dispatcher = Dispatcher.UIThread;
        var dispatcherType = dispatcher.GetType();

        var timersField = dispatcherType.GetField("_timers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (timersField?.GetValue(dispatcher) is IList timers)
        {
            var timerSnapshot = new object[timers.Count];
            timers.CopyTo(timerSnapshot, 0);

            foreach (var timer in timerSnapshot)
            {
                if (timer is null)
                {
                    continue;
                }

                var stopMethod = timer.GetType().GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                stopMethod?.Invoke(timer, null);

                var tickField = timer.GetType().GetField("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
                tickField?.SetValue(timer, null);
            }

            timers.Clear();
        }

        var timersVersionField = dispatcherType.GetField("_timersVersion", BindingFlags.Instance | BindingFlags.NonPublic);
        if (timersVersionField?.GetValue(dispatcher) is long version)
        {
            timersVersionField.SetValue(dispatcher, version + 1);
        }

        var queueField = dispatcherType.GetField("_queue", BindingFlags.Instance | BindingFlags.NonPublic);
        if (queueField?.GetValue(dispatcher) is object queue)
        {
            var queueType = queue.GetType();
            var priorityChainsField = queueType.GetField("_priorityChains", BindingFlags.Instance | BindingFlags.NonPublic);
            if (priorityChainsField?.GetValue(queue) is System.Collections.IDictionary chains)
            {
                chains.Clear();
            }

            var cacheField = queueType.GetField("_cacheReusableChains", BindingFlags.Instance | BindingFlags.NonPublic);
            if (cacheField?.GetValue(queue) is object cache)
            {
                cache.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.Invoke(cache, null);
            }

            SetField(queue, "_head", null);
            SetField(queue, "_tail", null);
        }
    }

    private static void ClearDragPreviewHelper()
    {
        var type = Type.GetType("Dock.Avalonia.Internal.DragPreviewHelper, Dock.Avalonia");
        if (type is null)
        {
            return;
        }

        var windowField = type.GetField("s_window", BindingFlags.Static | BindingFlags.NonPublic);
        if (windowField?.GetValue(null) is Window window)
        {
            window.Close();
        }

        SetStaticField(type, "s_window", null);
        SetStaticField(type, "s_control", null);
        SetStaticField(type, "s_managedControl", null);
        SetStaticField(type, "s_managedLayer", null);
        SetStaticField(type, "s_windowTemplatesInitialized", false);
        SetStaticField(type, "s_managedTemplatesInitialized", false);
    }

    private static void ClearDragPreviewContext()
    {
        var type = Type.GetType("Dock.Avalonia.Internal.DragPreviewContext, Dock.Avalonia");
        if (type is null)
        {
            return;
        }

        SetStaticField(type, "IsActive", false, isProperty: true);
        SetStaticField(type, "Dockable", null, isProperty: true);
    }

    private static void SetStaticField(Type type, string name, object? value, bool isProperty = false)
    {
        if (isProperty)
        {
            var property = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(null, value);
            return;
        }

        var field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, value);
    }

    private static object? GetAvaloniaService(Type serviceType)
    {
        var locatorType = typeof(AvaloniaLocator);
        var currentProperty = locatorType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var resolver = currentProperty?.GetValue(null);
        if (resolver is null)
        {
            return null;
        }

        var getService = resolver.GetType().GetMethod("GetService", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Type) }, null);
        return getService?.Invoke(resolver, new object?[] { serviceType });
    }

    private static object? GetTopLevelMouseDevice(TopLevel? topLevel)
    {
        if (topLevel?.PlatformImpl is null)
        {
            return null;
        }

        var property = topLevel.PlatformImpl.GetType().GetProperty("MouseDevice", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return property?.GetValue(topLevel.PlatformImpl);
    }

    private static void ClearPlatformImpl(TopLevel topLevel)
    {
        var platformImpl = topLevel.PlatformImpl;
        if (platformImpl is null)
        {
            return;
        }

        var mouseDevice = GetTopLevelMouseDevice(topLevel);
        if (mouseDevice is IDisposable disposableMouse)
        {
            disposableMouse.Dispose();
        }

        var implType = platformImpl.GetType();
        var inputRootProperty = implType.GetProperty("InputRoot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (inputRootProperty?.CanWrite == true)
        {
            inputRootProperty.SetValue(platformImpl, null);
        }

        var inputProperty = implType.GetProperty("Input", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (inputProperty?.CanWrite == true)
        {
            inputProperty.SetValue(platformImpl, null);
        }

        if (platformImpl is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static object? GetStaticMemberValue(Type type, string propertyName, string? fieldName = null)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (property is not null)
        {
            return property.GetValue(null);
        }

        if (fieldName is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field is not null)
            {
                return field.GetValue(null);
            }
        }

        return null;
    }

    private static void ScrubInputReferences()
    {
        var inputRootType = typeof(IInputRoot);
        var inputElementType = typeof(IInputElement);
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName is null)
            {
                continue;
            }

            var traceAll = string.Equals(
                Environment.GetEnvironmentVariable("DOCK_LEAK_TRACE_ALL"),
                "1",
                StringComparison.Ordinal);

            if (!traceAll
                && !assembly.FullName.StartsWith("Avalonia", StringComparison.Ordinal)
                && !assembly.FullName.StartsWith("Dock.", StringComparison.Ordinal))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type is not null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type is null)
                {
                    continue;
                }

                if (IsLeakTraceEnabled && type.FullName == "Avalonia.Utilities.WeakEvents.AvaloniaPropertyChanged")
                {
                    Trace($"[LeakTrace:WeakEvents] Found AvaloniaPropertyChanged in {assembly.FullName}.");
                }

                FieldInfo[] fields;
                try
                {
                    fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                }
                catch
                {
                    continue;
                }

                foreach (var field in fields)
                {
                    object? value;
                    try
                    {
                        value = field.GetValue(null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (value is null)
                    {
                        continue;
                    }

                    if (inputRootType.IsAssignableFrom(field.FieldType) || inputElementType.IsAssignableFrom(field.FieldType))
                    {
                        TrySetField(field, null, null);
                        continue;
                    }

                    if (value is IInputRoot || value is IInputElement)
                    {
                        TrySetField(field, null, null);
                        continue;
                    }

                    ScrubObject(value, inputRootType, inputElementType, visited, depth: 0);
                }
            }
        }
    }

    private static void ScrubObject(object target, Type inputRootType, Type inputElementType, HashSet<object> visited, int depth)
    {
        if (depth > 6)
        {
            return;
        }

        if (target is Delegate)
        {
            return;
        }

        if (!visited.Add(target))
        {
            return;
        }

        var targetType = target.GetType();
        if (targetType.IsPrimitive || targetType.IsEnum || targetType == typeof(string))
        {
            return;
        }

        if (target is IDictionary dictionary)
        {
            if (dictionary.Count == 0)
            {
                return;
            }

            var shouldClear = false;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is not null
                    && (inputRootType.IsInstanceOfType(entry.Key) || inputElementType.IsInstanceOfType(entry.Key)))
                {
                    shouldClear = true;
                    break;
                }

                if (entry.Value is not null
                    && (inputRootType.IsInstanceOfType(entry.Value) || inputElementType.IsInstanceOfType(entry.Value)))
                {
                    shouldClear = true;
                    break;
                }
            }

            if (shouldClear)
            {
                dictionary.Clear();
                return;
            }
        }

        if (target is IList list)
        {
            if (list.Count == 0)
            {
                return;
            }

            var shouldClear = false;
            foreach (var item in list)
            {
                if (item is null)
                {
                    continue;
                }

                if (inputRootType.IsInstanceOfType(item) || inputElementType.IsInstanceOfType(item))
                {
                    shouldClear = true;
                    break;
                }
            }

            if (shouldClear)
            {
                list.Clear();
                return;
            }
        }

        foreach (var field in targetType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (field.FieldType.IsValueType)
            {
                continue;
            }

            object? value;
            try
            {
                value = field.GetValue(target);
            }
            catch
            {
                continue;
            }

            if (value is null)
            {
                continue;
            }

            if (inputRootType.IsAssignableFrom(field.FieldType) || inputElementType.IsAssignableFrom(field.FieldType))
            {
                TrySetField(field, target, null);
                continue;
            }

            if (value is IInputRoot || value is IInputElement)
            {
                TrySetField(field, target, null);
                continue;
            }

            ScrubObject(value, inputRootType, inputElementType, visited, depth + 1);
        }
    }

    private static void TrySetField(FieldInfo field, object? target, object? value)
    {
        try
        {
            field.SetValue(target, value);
        }
        catch
        {
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        internal static readonly ReferenceEqualityComparer Instance = new();

        bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    // Dump helpers removed after investigation work; keep trace hooks lean for leak tests.

    internal static void ResetDispatcherForUnitTests()
    {
        var reset = typeof(Dispatcher).GetMethod("ResetForUnitTests", BindingFlags.Static | BindingFlags.NonPublic);
        reset?.Invoke(null, null);

        var resetBefore = typeof(Dispatcher).GetMethod("ResetBeforeUnitTests", BindingFlags.Static | BindingFlags.NonPublic);
        resetBefore?.Invoke(null, null);
    }

    internal static void Trace(string message)
    {
        if (!IsLeakTraceEnabled)
        {
            return;
        }

        Console.WriteLine(message);

        try
        {
            File.AppendAllText("/tmp/dock_leak_trace.log", message + Environment.NewLine);
        }
        catch
        {
        }
    }

    internal static void TraceVisualTree(Visual root, string label, int maxTypes = 60)
    {
        if (!IsLeakTraceEnabled)
        {
            return;
        }

        var templateInfo = string.Empty;
        if (root is TemplatedControl templated)
        {
            templateInfo = $" Template={(templated.Template is null ? "null" : templated.Template.GetType().Name)} Theme={(templated.Theme is null ? "null" : templated.Theme.GetType().Name)}";
        }

        var groups = root.GetVisualDescendants()
            .GroupBy(visual => visual.GetType().Name)
            .OrderByDescending(group => group.Count())
            .Take(maxTypes)
            .Select(group => $"{group.Key}={group.Count()}");

        Trace($"[LeakTrace:{label}] Root={root.GetType().Name} Attached={root.IsAttachedToVisualTree}{templateInfo} Visual descendants: {string.Join(", ", groups)}");
    }

}
