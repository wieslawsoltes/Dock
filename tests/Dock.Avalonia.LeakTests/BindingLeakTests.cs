using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class BindingLeakTests
{
    private sealed class BindingViewModel : INotifyPropertyChanged
    {
        private bool _isGlobalDockActive;

        public bool IsGlobalDockActive
        {
            get => _isGlobalDockActive;
            set
            {
                if (_isGlobalDockActive == value)
                {
                    return;
                }

                _isGlobalDockActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGlobalDockActive)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed record BindingLeakResult(WeakReference ViewModelRef, DockTarget Target);

    [ReleaseFact]
    public void Binding_Clears_DataContext_DoesNotLeak_ViewModel()
    {
        var result = RunInSession(() =>
        {
            var viewModel = new BindingViewModel { IsGlobalDockActive = true };

            var target = new DockTarget();
            target.Bind(DockTargetBase.IsGlobalDockActiveProperty,
                new Binding(nameof(BindingViewModel.IsGlobalDockActive)) { Mode = BindingMode.TwoWay });
            target.DataContext = viewModel;

            var window = new Window { Content = target };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            target.DataContext = null;
            DrainDispatcher();

            var result = new BindingLeakResult(new WeakReference(viewModel), target);
            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.ViewModelRef);
        System.GC.KeepAlive(result.Target);
    }
}
