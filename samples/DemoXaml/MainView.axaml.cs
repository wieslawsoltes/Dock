using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using AvaloniaDemo.Xaml.Json;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;

namespace AvaloniaDemo.Xaml;

public class MainView : UserControl
{
    private readonly IDockSerializer _serializer;
    private readonly IDockState _dockState;

    private readonly Dictionary<Type, List<string>> _properties;
    private readonly JsonSerializerOptions _options;

    public MainView()
    {
        InitializeComponent();

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
        };

        void ModifyTypeInfo(JsonTypeInfo ti)
        {
            if (ti.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            if (_properties.TryGetValue(ti.Type, out var properties))
            {
                //Console.WriteLine($"{ti.Type}");

                var allowedProperties = ti.Properties.Where(x => properties.Contains(x.Name)).ToList();

                ti.Properties.Clear();

                foreach (var property in allowedProperties)
                {
                    //Console.WriteLine($"  {property.Name}");
                    ti.Properties.Add(property);
                }
            }
            else
            {
                ti.Properties.Clear();
            }
        }

        // TODO:
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
        };

        _serializer = new DockSerializer(typeof(AvaloniaList<>));
        _dockState = new DockState();

        var dock = this.FindControl<DockControl>("Dock");
        if (dock is { })
        {
            var layout = dock.Layout;
            if (layout is { })
            {
                _dockState.Save(layout);
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private List<FilePickerFileType> GetOpenOpenLayoutFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private List<FilePickerFileType> GetSaveOpenLayoutFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private async Task OpenLayout()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open layout",
            FileTypeFilter = GetOpenOpenLayoutFileTypes(),
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null && file.CanOpenRead)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var dock = this.FindControl<DockControl>("Dock");
                if (dock is { })
                {
                    var layout = JsonSerializer.Deserialize<IDock?>(stream, _options);

                    // TODO:
                    // var layout = _serializer.Load<IDock?>(stream);
                    if (layout is { })
                    {
                        dock.Layout = layout;
                        _dockState.Restore(layout);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SaveLayout()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save layout",
            FileTypeChoices = GetSaveOpenLayoutFileTypes(),
            SuggestedFileName = "layout",
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null && file.CanOpenWrite)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                var dock = this.FindControl<DockControl>("Dock");
                if (dock?.Layout is { })
                {
                    // TODO:
                    // _serializer.Save(stream, dock.Layout);

                    // ReSharper disable once MethodHasAsyncOverload
                    JsonSerializer.Serialize(stream, dock.Layout, _options);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void CloseLayout()
    {
        var dock = this.FindControl<DockControl>("Dock");
        if (dock is { })
        {
            dock.Layout = null;
        }
    }

    private async void FileOpenLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        await OpenLayout();
    }

    private async void FileSaveLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        await SaveLayout();
    }

    private void FileCloseLayout_OnClick(object? sender, RoutedEventArgs e)
    {
        CloseLayout();
    }
}

