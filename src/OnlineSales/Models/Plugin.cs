// <copyright file="Plugin.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using OnlineSales.Interfaces;

namespace OnlineSales.Models
{
    public class Plugin
    {
        private readonly IPlugin entryPoint;
        private readonly Assembly @assembly;

        public Plugin(string fullPath)
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

            @assembly = asm;
            this.entryPoint = entrypoint;
        }

        public IPlugin Interface
        {
            get
            {
                return entryPoint;
            }
        }

        public Assembly Assembly
        {
            get
            {
                return @assembly;
            }
        }
    }
}
