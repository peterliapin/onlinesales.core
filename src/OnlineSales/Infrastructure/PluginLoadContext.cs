// <copyright file="PluginLoadContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Runtime.Loader;

namespace OnlineSales.Infrastructure;

internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;

    public PluginLoadContext(string pluginPath)
    {
        this.resolver = new AssemblyDependencyResolver(pluginPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = this.resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null
            ? this.LoadFromAssemblyPath(assemblyPath)
            : null!;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null
            ? this.LoadUnmanagedDllFromPath(libraryPath)
            : nint.Zero;
    }
}