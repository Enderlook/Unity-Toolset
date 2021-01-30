using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Compilation;

using SystemAssembly = System.Reflection.Assembly;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling
{
    internal static class AssembliesHelper
    {
        private static HashSet<SystemAssembly> assemblies;

        /// <summary>
        /// Get all assemblies from <see cref="AppDomain.CurrentDomain"/> which are in the <see cref="CompilationPipeline.GetAssemblies(AssembliesType)"/> either <see cref="AssembliesType.Editor"/> and <see cref="AssembliesType.Player"/>.
        /// </summary>
        /// <param name="ingoreCache">Whenever it should recalculate the value regardless the cache.</param>
        /// <returns>Assemblies which matches criteria.</returns>
        public static IReadOnlyCollection<SystemAssembly> GetAllAssembliesOfPlayerAndEditorAssemblies(bool ingoreCache = false)
        {
            // Cached because it takes like 100ms to do.
            if (assemblies == null || ingoreCache)
            {
                UnityAssembly[] unityAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor).Concat(CompilationPipeline.GetAssemblies(AssembliesType.Player)).ToArray();
                assemblies = new HashSet<SystemAssembly>();
                foreach (SystemAssembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name = assembly.GetName().Name;
                    foreach (UnityAssembly unityAssembly in unityAssemblies)
                    {
                        if (name == unityAssembly.name)
                            assemblies.Add(assembly);
                    }
                }
            }
            return assemblies;
        }
    }
}