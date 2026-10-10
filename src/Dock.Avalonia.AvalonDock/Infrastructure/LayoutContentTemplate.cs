using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Dock.Model.Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// Selects compiled presenters for ordinary AvalonDock content while retaining
// Dock's native recycling and control-detachment path for explicit controls.
internal sealed class LayoutContentTemplate : IRecyclingDataTemplate
{
    public IDataTemplate? DocumentTemplate { get; set; }
    public IDataTemplate? ToolTemplate { get; set; }

    public bool Match(object? data) => data is Document or Tool;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        object? content = data switch { Document document => document.Content, Tool tool => tool.Content, _ => null };
        if (content is null or Control or ITemplate or IDataTemplate or Func<IServiceProvider, object>)
            return (data as IRecyclingDataTemplate)?.Build(data, existing);
        if (data is Document)
        {
            if (existing is ContentControl { DataContext: Document } documentPresenter && documentPresenter.Classes.Contains("avalondock-content")) return existing;
            return DocumentTemplate?.Build(data);
        }
        if (existing is ContentControl { DataContext: Tool } toolPresenter && toolPresenter.Classes.Contains("avalondock-content")) return existing;
        return ToolTemplate?.Build(data);
    }
}
