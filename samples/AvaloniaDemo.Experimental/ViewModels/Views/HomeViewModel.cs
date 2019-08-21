using System;
using Dock.Model;

namespace AvaloniaDemo.ViewModels.Views
{
    public class HomeViewModel : DockBase
    {
        public override IDockable Clone()
        {
            var homeViewModel = new HomeViewModel();

            CloneHelper.CloneDockProperties(this, homeViewModel);

            return homeViewModel;
        }
    }
}
