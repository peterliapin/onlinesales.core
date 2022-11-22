// <copyright file="Program.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

Console.WriteLine("Target CleanPluginAssemblies");

string hostBuildFolder = args[0].Trim("'\"".ToCharArray());

string pluginBuildOutputFolder = args[1].Trim("'\"".ToCharArray());

var hostBuildFolderInfo = new DirectoryInfo(hostBuildFolder);

var hostFiles = hostBuildFolderInfo.GetFiles().Select(f => f.Name).ToList();

var pluginBuildFolderInfo = new DirectoryInfo(pluginBuildOutputFolder);

FileInfo[] pluginFilesInfo = pluginBuildFolderInfo.GetFiles();

foreach (var pluginFileInfo in pluginFilesInfo.Where(f => hostFiles.Contains(f.Name)))
{
    Console.WriteLine("\tDeleting file: \"" + pluginFileInfo.FullName + "\"");

    pluginFileInfo.Delete();
}