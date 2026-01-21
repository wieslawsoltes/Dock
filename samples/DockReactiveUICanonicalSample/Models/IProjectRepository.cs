using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockReactiveUICanonicalSample.Models;

public interface IProjectRepository
{
    IReadOnlyList<Project> GetProjects();

    Project? GetProject(string id);

    Task<IReadOnlyList<Project>> GetProjectsAsync();

    Task<IReadOnlyList<ProjectFile>> GetProjectFilesAsync(string projectId);
}
