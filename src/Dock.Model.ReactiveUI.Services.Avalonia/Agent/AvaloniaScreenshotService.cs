using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Dock.Model.ReactiveUI.Services.Agent;

namespace Dock.Model.ReactiveUI.Services.Avalonia.Agent;

/// <summary>
/// Captures Avalonia main window screenshots as PNG artifacts for agent execution evidence.
/// </summary>
public sealed class AvaloniaScreenshotService : IAgentScreenshotService
{
    private readonly Func<Window?> _getMainWindow;
    private readonly string _artifactRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaScreenshotService"/> class.
    /// </summary>
    /// <param name="getMainWindow">A function that returns the current main window.</param>
    /// <param name="artifactRoot">The root directory for captured artifacts.</param>
    public AvaloniaScreenshotService(Func<Window?> getMainWindow, string artifactRoot)
    {
        _getMainWindow = getMainWindow ?? throw new ArgumentNullException(nameof(getMainWindow));
        _artifactRoot = string.IsNullOrWhiteSpace(artifactRoot)
            ? throw new ArgumentException("Artifact root is required.", nameof(artifactRoot))
            : artifactRoot;
    }

    /// <inheritdoc />
    public async Task<string> CaptureMainWindowAsync(string executionId, string name, CancellationToken cancellationToken = default)
    {
        return await Dispatcher.UIThread.InvokeAsync(
            () => CaptureMainWindow(executionId, name, cancellationToken),
            DispatcherPriority.Render,
            cancellationToken);
    }

    private string CaptureMainWindow(string executionId, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var window = _getMainWindow() ?? throw new InvalidOperationException("Main window is not available.");
        var width = Math.Max(1, (int)Math.Ceiling(window.Bounds.Width));
        var height = Math.Max(1, (int)Math.Ceiling(window.Bounds.Height));
        var pixelSize = new PixelSize(width, height);
        var dpi = new Vector(96, 96);

        using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
        bitmap.Render(window);

        var safeName = SanitizeFileName(name);
        var directory = Path.Combine(_artifactRoot, executionId, "screenshots");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss_fff}_{safeName}.png");
        bitmap.Save(path);
        return path;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "screenshot";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            for (var j = 0; j < invalid.Length; j++)
            {
                if (chars[i] == invalid[j])
                {
                    chars[i] = '_';
                    break;
                }
            }
        }

        return new string(chars);
    }
}
