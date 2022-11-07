// <copyright file="PluginManager.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Reflection;
using OnlineSales.Models;
using Serilog;

namespace OnlineSales.Backend.Infrastructure
{
    public static class PluginManager
    {
        private static readonly string PluginsFolder = "plugins";
        private static readonly List<Plugin> PluginList = new List<Plugin>();

        public static void Init()
        {
            if (!Directory.Exists(PluginsFolder))
            {
                Directory.CreateDirectory(PluginsFolder);
            }

            var paths = Directory.GetFiles(PluginsFolder, "*.dll").Select(p => Path.GetFullPath(p)).ToArray();
            foreach (var path in paths)
            {
                try
                {
                    PluginList.Add(new Plugin(path));
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(ex, "[PluginManager][Error]");
                }
            }

            foreach (var plugin in PluginList)
            {
                plugin.Interface.OnInitialize().Wait();
            }
        }

        public static void Shutdown()
        {
            foreach (var plugin in PluginList)
            {
                plugin.Interface.OnShutdown().Wait();
            }
        }

        public static List<Plugin> GetPluginList()
        {
            return PluginList;
        }

        public static string[] GetPluginsSettingsPaths()
        {
            return PluginList.Select(p => p.Interface.SettingsPath).ToArray();
        }
    }
}
