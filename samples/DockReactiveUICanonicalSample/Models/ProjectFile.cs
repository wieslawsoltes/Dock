namespace DockReactiveUICanonicalSample.Models;

public sealed class ProjectFile
{
    public ProjectFile(string id, string name, string path, string fileType)
    {
        Id = id;
        Name = name;
        Path = path;
        FileType = fileType;
    }

    public string Id { get; }

    public string Name { get; }

    public string Path { get; }

    public string FileType { get; }
}
