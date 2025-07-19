namespace Dock.Model.Core;

public readonly struct DockOperationResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    public DockOperationResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}

