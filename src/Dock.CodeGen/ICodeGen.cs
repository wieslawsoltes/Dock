using Dock.Model;

namespace Dock.CodeGen
{
    public interface ICodeGen
    {
        void Generate(IView view, string path);
    }
}
