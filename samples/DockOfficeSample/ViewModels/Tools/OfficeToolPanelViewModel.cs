using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockOfficeSample.Models;

namespace DockOfficeSample.ViewModels.Tools;

public class OfficeToolPanelViewModel : Tool
{
    public OfficeToolPanelViewModel(OfficeAppKind appKind, string subtitle, IReadOnlyList<string> items)
    {
        AppKind = appKind;
        Subtitle = subtitle;
        Items = new ObservableCollection<string>(items);
    }

    public OfficeAppKind AppKind { get; }
    public string Subtitle { get; }
    public ObservableCollection<string> Items { get; }
}
