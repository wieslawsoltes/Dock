using System.Reflection;
using Xunit;

[assembly: AssemblyTitle("Dock.Avalonia.HeadlessTests")]

// Don't run tests in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
