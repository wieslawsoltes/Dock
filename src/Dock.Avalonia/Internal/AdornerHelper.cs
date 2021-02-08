using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal
{
    internal class AdornerHelper
    {
        public Control? Adorner;

        public void AddAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);
            if (layer == null)
            {
                return;
            }
            
            if (Adorner != null)
            {
                layer.Children.Remove(Adorner);
                Adorner = null;
            }

            Adorner = new DockTarget
            {
                [AdornerLayer.AdornedElementProperty] = visual,
            };

            ((ISetLogicalParent) Adorner).SetParent(visual as ILogical);

            layer.Children.Add(Adorner);
        }

        public void RemoveAdorner(IVisual visual)
        {
            var layer = AdornerLayer.GetAdornerLayer(visual);
            if (layer != null)
            {
                if (Adorner != null)
                {
                    layer.Children.Remove(Adorner);
                    ((ISetLogicalParent) Adorner).SetParent(null);
                    Adorner = null;
                }
            }
        }
    }
}
