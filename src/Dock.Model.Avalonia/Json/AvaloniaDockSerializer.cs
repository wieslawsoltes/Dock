using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Json;

/// <summary>
/// 
/// </summary>
public class AvaloniaDockSerializer : IDockSerializer
{
    // TODO:
    /*
    /// <summary>
    /// Dock json serializer default context.
    /// </summary>
    public static readonly DockJsonContext s_serializerContext = new(
        new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            IncludeFields = false,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        });
    */
    
    private readonly Dictionary<Type, List<string>> _properties;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// 
    /// </summary>
    public AvaloniaDockSerializer()
    {
        _properties = new()
        {
            [typeof(IDockable)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(IDocument)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(IDocumentContent)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(ITool)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(IToolContent)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(IDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
            },
            [typeof(IDockDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IDockDock
                "LastChildFill",
            },
            [typeof(IDocumentDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IDocumentDock
                "CanCreateDocument",
            },
            [typeof(IDocumentDockContent)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
            },
            [typeof(IProportionalDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IProportionalDock
                "Orientation",
            },
            [typeof(IProportionalDockSplitter)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
            },
            [typeof(IRootDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IRootDock
                "IsFocusableRoot",
                "HiddenDockables",
                "LeftPinnedDockables",
                "RightPinnedDockables",
                "TopPinnedDockables",
                "BottomPinnedDockables",
                "Window",
                "Windows",
            },
            [typeof(IToolDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IToolDock
                "Alignment",
                "IsExpanded",
                "AutoHide",
                "GripMode",
            },
            [typeof(DockableBase)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(DockBase)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
            },
            [typeof(Document)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(Tool)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
            },
            [typeof(DockDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IDockDock
                "LastChildFill",
            },
            [typeof(DocumentDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IDocumentDock
                "CanCreateDocument",
            },
            [typeof(ProportionalDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IProportionalDock
                "Orientation",
            },
            [typeof(ProportionalDockSplitter)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
            },
            [typeof(RootDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IRootDock
                "IsFocusableRoot",
                "HiddenDockables",
                "LeftPinnedDockables",
                "RightPinnedDockables",
                "TopPinnedDockables",
                "BottomPinnedDockables",
                "Window",
                "Windows",
            },
            [typeof(ToolDock)] = new List<string>
            {
                // IDockable
                "Id",
                "Title",
                "Context",
                "CanClose",
                "CanPin",
                "CanFloat",
                // IDock
                "VisibleDockables",
                "ActiveDockable",
                "DefaultDockable",
                "FocusedDockable",
                "Proportion",
                "Dock",
                "IsActive",
                "IsCollapsable",
                // IToolDock
                "Alignment",
                "IsExpanded",
                "AutoHide",
                "GripMode",
            },
            [typeof(IDockWindow)] = new List<string>
            {
                // IDockWindow
                "Id",
                "X",
                "Y",
                "Width",
                "Height",
                "Topmost",
                "Title",
                "Layout",
            },
            [typeof(DockWindow)] = new List<string>
            {
                // IDockWindow
                "Id",
                "X",
                "Y",
                "Width",
                "Height",
                "Topmost",
                "Title",
                "Layout",
            },
        };

        void ModifyTypeInfo(JsonTypeInfo ti)
        {
            if (ti.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            if (_properties.TryGetValue(ti.Type, out var properties))
            {
                // Console.WriteLine($"{ti.Type}");

                var allowedProperties = ti.Properties.Where(x => properties.Contains(x.Name)).ToList();

                ti.Properties.Clear();

                foreach (var property in allowedProperties)
                {
                    // Console.WriteLine($"  {property.Name}");
                    ti.Properties.Add(property);
                }
            }
            else
            {
                ti.Properties.Clear();
            }
        }

        // TODO: Handle deserializing AvaloniaList<T>

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            IncludeFields = false,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            TypeInfoResolver = new AvaloniaModelPolymorphicTypeResolver
            {
                Modifiers = { ModifyTypeInfo }
            },
            Converters =
            {
                // TODO:
                // new JsonConverterFactoryAvaloniaList()
            }
        };
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text, _options);
    }

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}
