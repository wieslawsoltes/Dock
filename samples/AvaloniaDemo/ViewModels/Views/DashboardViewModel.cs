using Dock.Model;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;

namespace AvaloniaDemo.ViewModels.Views
{
    public class DashboardViewModel : DockBase
    {
        public override IDockable Clone()
        {
            var dashboardViewModel = new DashboardViewModel();

            CloneHelper.CloneDockProperties(this, dashboardViewModel);

            return dashboardViewModel;
        }
    }
}
