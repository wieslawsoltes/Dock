using System.Collections.Generic;

namespace DockReactiveUICanonicalSample.Models;

public interface IProjectRepository
{
    IReadOnlyList<Project> GetProjects();

    Project? GetProject(string id);
}
