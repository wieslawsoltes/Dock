using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dock.Model.Core;

namespace Dock.Model.Plugins;

/// <summary>
/// Provides helpers for loading <see cref="IDockModule"/> implementations.
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// Instantiates all <see cref="IDockModule"/> implementations from the given assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>The instantiated modules.</returns>
    public static IEnumerable<IDockModule> LoadModules(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null)!;
            }

            foreach (var type in types)
            {
                if (typeof(IDockModule).IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    if (Activator.CreateInstance(type) is IDockModule module)
                    {
                        yield return module;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Loads and registers modules from the given assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <param name="factory">Factory to register dockables with.</param>
    public static void RegisterModules(IEnumerable<Assembly> assemblies, IFactory factory)
    {
        foreach (var module in LoadModules(assemblies))
        {
            module.Register(factory);
        }
    }
}

