using System.Collections.Generic;

namespace DockOfficeSample.Models;

public sealed record OfficeRibbonGroup(string Title, IReadOnlyList<string> Actions);
