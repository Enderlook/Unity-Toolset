using System;
using System.Collections.Generic;

using UnityEditor.Callbacks;
using UnityEditor.Compilation;

using SystemAssembly = System.Reflection.Assembly;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling
{
    internal static class AssembliesHelper
    {
        private static HashSet<SystemAssembly> assemblies;

        [DidReloadScripts(-10)]
        private static void Reset() => assemblies = null;

        /// <summary>
        /// Get all assemblies from <see cref="AppDomain.CurrentDomain"/> which are in the <see cref="CompilationPipeline.GetAssemblies(AssembliesType)"/> either <see cref="AssembliesType.Editor"/> or <see cref="AssembliesType.Player"/>.
        /// </summary>
        /// <returns>Assemblies which matches criteria.</returns>
        public static IReadOnlyCollection<SystemAssembly> GetAllAssembliesOfPlayerAndEditorAssemblies()
        {
            // Cached because it takes like 100ms to do.
            if (assemblies == null)
            {
                UnityAssembly[] editorAsemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
                UnityAssembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
                assemblies = new HashSet<SystemAssembly>();
                foreach (SystemAssembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name = assembly.GetName().Name;
                    foreach (UnityAssembly unityAssembly in editorAsemblies)
                    {
                        if (name == unityAssembly.name)
                        {
                            assemblies.Add(assembly);
                            goto next;
                        }
                    }
                    foreach (UnityAssembly unityAssembly in playerAssemblies)
                    {
                        if (name == unityAssembly.name)
                        {
                            assemblies.Add(assembly);
                            goto next;
                        }
                    }
                next:;
                }
            }
            return assemblies;
        }
    }
}