using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockOfficeSample.Models;

namespace DockOfficeSample.ViewModels.Tools;

public class OfficeRibbonToolViewModel : Tool
{
    public OfficeRibbonToolViewModel(OfficeAppKind appKind, IReadOnlyList<string> tabs, IReadOnlyList<OfficeRibbonGroup> groups)
    {
        AppKind = appKind;
        Tabs = new ObservableCollection<string>(tabs);
        Groups = new ObservableCollection<OfficeRibbonGroup>(groups);
    }

    public OfficeAppKind AppKind { get; }
    public ObservableCollection<string> Tabs { get; }
    public ObservableCollection<OfficeRibbonGroup> Groups { get; }
}
