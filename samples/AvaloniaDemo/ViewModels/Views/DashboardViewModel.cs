using System;
using Dock.Model;

namespace AvaloniaDemo.ViewModels.Views
{
    public class DashboardViewModel : DockBase
    {
        public override IDockable Clone()
        {
            var dashboardViewModel = new HomeViewModel();

            CloneHelper.CloneDockProperties(this, dashboardViewModel);

            return dashboardViewModel;
        }
    }
}
