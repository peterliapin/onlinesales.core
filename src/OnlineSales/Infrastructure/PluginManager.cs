// <copyright file="PluginManager.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure;

public static class PluginManager
{
    private static readonly string PluginsFolder = Path.Combine(AppContext.BaseDirectory, "plugins");
    private static readonly List<IPlugin> PluginList = new List<IPlugin>();

    public static void Init(IConfigurationBuilder configurationBuilder)
    {
        var pluginsDirectory = new DirectoryInfo(PluginsFolder);

        if (pluginsDirectory.Exists)
        {
            Log.Information("Loading plugins from the folder {0}", PluginsFolder);

            LoadPlugins(pluginsDirectory, configurationBuilder);
        }
        else
        {
            Log.Information("Plugins folder does not exists ({0})", PluginsFolder);
        }
    }

    public static void Init(IApplicationBuilder application)
    {
        var applicationPlugins = from plugin in PluginList
                                 where plugin is IPluginApplication
                                 select plugin as IPluginApplication;

        foreach (var appPlugin in applicationPlugins)
        {
            appPlugin.ConfigureApplication(application);
        }
    }

    public static List<IPlugin> GetPluginList()
    {
        return PluginList;
    }

    private static void LoadPlugins(DirectoryInfo pluginsDirectory, IConfigurationBuilder configurationBuilder)
    {
        foreach (var pluginDirectory in pluginsDirectory.GetDirectories())
        {
            var pluginDllName = pluginDirectory.Name + ".dll";

            var pluginInfo = pluginDirectory.GetFiles(pluginDllName).FirstOrDefault();
            if (pluginInfo != null)
            {
                try
                {
                    var plugin = LoadPlugin(pluginInfo.FullName);

                    // we'll add plugin configuration json file to the configurationBuilder only if plugin has been loaded.
                    MergePluginConfig(pluginDirectory, configurationBuilder);

                    PluginList.Add(plugin);

                    Log.Information("Plugin {0} successfully loaded from {1}", pluginDllName, pluginDirectory.FullName);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[PluginManager][Error]");
                }
            }
            else
            {
                Log.Warning("Plugin directory {0} does not have a plugin DLL named {1}", pluginDirectory.FullName, pluginDllName);
            }
        }
    }

    private static IPlugin LoadPlugin(string fullPluginDllPath)
    {
        var fileName = Path.GetFileName(fullPluginDllPath);

        var loadContext = new PluginLoadContext(fullPluginDllPath);

        var asm = loadContext.LoadFromAssemblyPath(fullPluginDllPath);
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

    private static void MergePluginConfig(DirectoryInfo pluginDirectory, IConfigurationBuilder configurationBuilder)
    {
        var pluginSettingsInfo = pluginDirectory.GetFiles("pluginsettings.json").FirstOrDefault();

        if (pluginSettingsInfo != null)
        {
            Log.Information($"Loading plugin settings from {pluginSettingsInfo.FullName}");
            configurationBuilder.AddJsonFile(pluginSettingsInfo.FullName);
        }
    }
}