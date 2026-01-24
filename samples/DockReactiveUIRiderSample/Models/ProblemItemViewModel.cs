namespace DockReactiveUIRiderSample.Models;

public class ProblemItemViewModel
{
    public ProblemItemViewModel(string severity, string message)
    {
        Severity = severity;
        Message = message;
    }

    public string Severity { get; }

    public string Message { get; }
}
