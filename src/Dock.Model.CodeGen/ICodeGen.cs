using Dock.Model;

namespace Dock.Model.CodeGen
{
    public interface ICodeGen
    {
        void Generate(IView view, string path);
    }
}
