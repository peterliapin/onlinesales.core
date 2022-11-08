// <copyright file="PluginManager.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Reflection;
using System.Runtime.Loader;
using OnlineSales.Interfaces;
using Serilog;

namespace OnlineSales.Backend.Infrastructure;
public static class PluginManager
{
    private static readonly string PluginsFolder = "plugins";
    private static readonly List<IPlugin> PluginList = new List<IPlugin>();

    public static void Init()
    {
        if (!Directory.Exists(PluginsFolder))
        {
            return;
        }

        var paths = Directory.GetFiles(PluginsFolder, "*.dll").Select(p => Path.GetFullPath(p)).ToArray();
        foreach (var path in paths)
        {
            try
            {
                PluginList.Add(LoadPlugin(path));
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "[PluginManager][Error]");
            }
        }

        foreach (var plugin in PluginList)
        {
            plugin.OnInitialize().Wait();
        }
    }

    public static List<IPlugin> GetPluginList()
    {
        return PluginList;
    }

    public static string[] GetPluginsSettingsPaths()
    {
        return PluginList.Select(p => p.SettingsPath).ToArray();
    }

    private static IPlugin LoadPlugin(string fullPath)
    {
        var fileName = Path.GetFileName(fullPath);
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        if (asm == null)
        {
            throw new InvalidProgramException($"Failed to load plugin '{fileName}'");
        }

        var entryPointType = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && t != typeof(IPlugin));
        if (entryPointType == null)
        {
            throw new InvalidProgramException($"Could not locate entry point of plugin '{fileName}'");
        }

        var entrypoint = Activator.CreateInstance(entryPointType) as IPlugin;
        if (entrypoint == null)
        {
            throw new InvalidProgramException($"Failed to init plugin '{fileName}'");
        }

        return entrypoint;
    }
}
