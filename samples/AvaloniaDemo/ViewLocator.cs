using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI;

namespace AvaloniaDemo
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            var name = data.GetType().FullName?.Replace("ViewModel", "View");
            if (name is null)
            {
                return new TextBlock { Text = "Invalid Data Type" };
            }
            var type = Type.GetType(name);
            if (type is { })
            {
                var instance = Activator.CreateInstance(type);
                if (instance is { })
                {
                    return (Control)instance;
                }
                else
                {
                    return new TextBlock { Text = "Create Instance Failed: " + type.FullName };
                }
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ReactiveObject || data is IDockable;
        }
    }
}
