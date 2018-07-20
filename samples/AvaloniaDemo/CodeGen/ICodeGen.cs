using Dock.Model;

namespace AvaloniaDemo.CodeGen
{
    public interface ICodeGen
    {
        void Generate(IView view, string path);
    }
}
