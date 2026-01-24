using System.Collections.ObjectModel;
using Dock.Model.ReactiveUI.Controls;
using DockFigmaSample.Models;

namespace DockFigmaSample.ViewModels.Tools;

public class CommentsToolViewModel : Tool
{
    public ObservableCollection<CommentItem> Comments { get; } = SampleData.CreateComments();
}
