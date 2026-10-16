using System.Collections.Generic;
using Avalonia.Controls;
using ReactiveUI.Reactive;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class MenuControlFixture : Window
{
    public MenuControlFixture() => InitializeComponent();
}

public sealed class SnapshotMenuSource : ReactiveObject
{
    public List<string> Values { get; } = new() { "First" };
    public string[] Entries => Values.ToArray();
}
