# Tab reordering

Dock allows dragging document and tool tabs within the tab strip to change their order. The tab follows the pointer as soon as it moves beyond the minimum drag distance defined by `DockSettings.MinimumHorizontalDragDistance` and `DockSettings.MinimumVerticalDragDistance`.

While the pointer stays within the original tab strip and has not travelled further than `DockSettings.MinimumTabFloatDistance`, the dragged tab is simply moved inside the strip. The tab position updates when the pointer crosses half of the neighbouring tab.

Once the drag distance exceeds `DockSettings.MinimumTabFloatDistance` the tab detaches from the strip and can be docked elsewhere or floated in its own window.

