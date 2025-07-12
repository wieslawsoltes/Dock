using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display pinned dock content when using floating overlay.
/// </summary>
public class PinnedDockWindow : Window
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(PinnedDockWindow);

    /// <summary>
    /// Play a fade-in animation when the window is shown.
    /// </summary>
    public async Task FadeInAsync()
    {
        Opacity = 0;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(OpacityProperty, 0d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, 1d) }
                }
            }
        };
        await animation.RunAsync(this);
    }

    /// <summary>
    /// Fade the window out and close it.
    /// </summary>
    public async Task FadeOutAndCloseAsync()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(OpacityProperty, 1d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, 0d) }
                }
            }
        };
        await animation.RunAsync(this);
        Close();
    }
}
