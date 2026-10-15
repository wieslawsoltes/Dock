using System;

namespace AvalonDock.Infrastructure;

// Presentations must not change selection inside an earlier collection
// observer, before Avalonia's own item and selection observers have run.
internal interface ILayoutChildrenNotification
{
    bool IsNotifying { get; }
    event Action? NotificationCompleted;
}
