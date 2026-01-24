namespace DockOfficeSample.Models;

public sealed record OfficeRecentFile(
    OfficeAppKind AppKind,
    string FileName,
    string Description,
    string UpdatedLabel);
