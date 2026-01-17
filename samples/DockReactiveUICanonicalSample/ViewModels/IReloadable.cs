using System.Threading.Tasks;

namespace DockReactiveUICanonicalSample.ViewModels;

public interface IReloadable
{
    Task ReloadAsync();
}
