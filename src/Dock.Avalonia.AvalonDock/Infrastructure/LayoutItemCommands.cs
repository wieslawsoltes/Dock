using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Diagnostics;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// Definitions follow the protected hooks; command lifetime follows attachment.
// Reinitializing a definition does not invalidate a command still held by a caller.
internal sealed class LayoutItemCommands(LayoutItem owner, params StyledProperty<ICommand?>[] properties) : IDisposable
{
    private readonly LayoutItemCommand?[] _defaults = new LayoutItemCommand?[properties.Length];
    private readonly List<LayoutItemCommand> _owned = new(properties.Length);

    public void Add(StyledProperty<ICommand?> property, Action execute, Func<bool> canExecute)
    {
        for (int index = 0; index < properties.Length; index++)
        {
            if (properties[index] != property) continue;
            LayoutItemCommand command = new(execute, canExecute);
            _owned.Add(command);
            _defaults[index] = command;
            return;
        }
        throw new ArgumentException("The command property is not registered with this container.", nameof(property));
    }

    public void Apply()
    {
        for (int index = 0; index < properties.Length; index++)
            if (owner.GetValue(properties[index]) is null)
                owner.SetCurrentValue(properties[index], _defaults[index]);
    }

    public void Clear()
    {
        for (int index = 0; index < properties.Length; index++)
        {
            StyledProperty<ICommand?> property = properties[index];
            if (!ReferenceEquals(owner.GetValue(property), _defaults[index])) continue;
            if (BindingOperations.GetBindingExpressionBase(owner, property) is { } binding &&
                owner.GetDiagnostic(property).Priority == BindingPriority.LocalValue)
                binding.Dispose();
            owner.SetCurrentValue(property, null);
        }
    }

    public void ClearDefinitions() => Array.Clear(_defaults, 0, _defaults.Length);

    public void Refresh()
    {
        // Callbacks can reinitialize or release commands. Newly created commands
        // answer CanExecute live and join the next notification pass.
        int count = _owned.Count;
        for (int index = 0; index < count && index < _owned.Count; index++) _owned[index].Refresh();
    }

    public void Dispose()
    {
        ClearDefinitions();
        for (int index = 0; index < _owned.Count; index++) _owned[index].Dispose();
        _owned.Clear();
    }
}
