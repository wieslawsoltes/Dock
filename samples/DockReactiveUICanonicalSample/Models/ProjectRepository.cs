using System.Collections.Generic;
using System.Linq;

namespace DockReactiveUICanonicalSample.Models;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly IReadOnlyList<Project> _projects;

    public ProjectRepository()
    {
        _projects = new List<Project>
        {
            new Project(
                "atlas",
                "Atlas",
                "Core platform services and shared infrastructure.",
                new List<ProjectFile>
                {
                    new ProjectFile("atlas.readme", "README.md", "/atlas/README.md", "Markdown"),
                    new ProjectFile("atlas.config", "appsettings.json", "/atlas/appsettings.json", "Json"),
                    new ProjectFile("atlas.pipeline", "pipeline.yml", "/atlas/build/pipeline.yml", "Yaml"),
                    new ProjectFile("atlas.service", "Service.cs", "/atlas/src/Service.cs", "CSharp")
                }),
            new Project(
                "lumen",
                "Lumen",
                "Realtime analytics and ingestion pipelines.",
                new List<ProjectFile>
                {
                    new ProjectFile("lumen.readme", "README.md", "/lumen/README.md", "Markdown"),
                    new ProjectFile("lumen.ingest", "Ingestor.cs", "/lumen/src/Ingestor.cs", "CSharp"),
                    new ProjectFile("lumen.rules", "rules.json", "/lumen/config/rules.json", "Json"),
                    new ProjectFile("lumen.pipeline", "pipeline.yml", "/lumen/build/pipeline.yml", "Yaml")
                }),
            new Project(
                "harbor",
                "Harbor",
                "Client tools and onboarding workflows.",
                new List<ProjectFile>
                {
                    new ProjectFile("harbor.readme", "README.md", "/harbor/README.md", "Markdown"),
                    new ProjectFile("harbor.shell", "ShellView.axaml", "/harbor/src/ShellView.axaml", "Xaml"),
                    new ProjectFile("harbor.state", "StateStore.cs", "/harbor/src/StateStore.cs", "CSharp"),
                    new ProjectFile("harbor.tasks", "tasks.json", "/harbor/config/tasks.json", "Json")
                })
        };
    }

    public IReadOnlyList<Project> GetProjects() => _projects;

    public Project? GetProject(string id)
        => _projects.FirstOrDefault(project => project.Id == id);
}
