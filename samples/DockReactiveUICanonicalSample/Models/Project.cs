using System.Collections.Generic;

namespace DockReactiveUICanonicalSample.Models;

public sealed class Project
{
    public Project(string id, string name, string description, IReadOnlyList<ProjectFile> files)
    {
        Id = id;
        Name = name;
        Description = description;
        Files = files;
    }

    public string Id { get; }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyList<ProjectFile> Files { get; }
}
