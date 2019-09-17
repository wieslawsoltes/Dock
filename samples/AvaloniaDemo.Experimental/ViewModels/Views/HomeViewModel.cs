using System;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo.ViewModels.Views
{
    public class HomeViewModel : RootDock
    {
        public override IDockable Clone()
        {
            var homeViewModel = new HomeViewModel();

            CloneHelper.CloneDockProperties(this, homeViewModel);
            CloneHelper.CloneRootDockProperties(this, homeViewModel);

            return homeViewModel;
        }
    }
}
