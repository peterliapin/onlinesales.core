// <copyright file="PluginLoadContext.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure;

internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;

    public PluginLoadContext(string pluginPath)
    {
        resolver = new AssemblyDependencyResolver(pluginPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null!;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}