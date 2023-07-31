// <copyright file="VstoVariablesProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Newtonsoft.Json;

namespace OnlineSales.Plugin.Vsto;

public class VstoVariablesProvider : IVariablesProvider
{
    public Dictionary<string, string> GetVariables(string language)
    {
        var exeFiles = Directory.GetFiles(VstoPlugin.VstoLocalPath, "*.exe", SearchOption.AllDirectories);

        var variables = new Dictionary<string, string>();

        foreach (var exeFile in exeFiles)
        {
            var relativeLocalPath = exeFile.Replace(VstoPlugin.VstoLocalPath, string.Empty);
            var relativeUrl = VstoPlugin.Configuration.Vsto.RequestPath + relativeLocalPath.Replace("\\", "/");

            variables[relativeLocalPath] = relativeUrl;
        }

        var vstoFiles = Directory.GetFiles(VstoPlugin.VstoLocalPath, "*.vsto", SearchOption.AllDirectories);

        var jsonVersionList = vstoFiles
            .Where(path => path.Contains("Application Files"))
            .Select(path =>
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(path));
                var version = string.Join(".", folderName!.Split('_').TakeLast(4).ToArray());
                var name = string.Join(" ", folderName.Split('_').Reverse().Skip(4).Reverse().ToArray());
                var relativeLocalPath = path.Replace(VstoPlugin.VstoLocalPath, string.Empty);
                var releaseDate = File.GetCreationTimeUtc(Path.ChangeExtension(path, ".dll"));
                if (releaseDate.Year < 2000)
                {
                    releaseDate = File.GetCreationTimeUtc(Path.ChangeExtension(path, ".dll.deploy"));
                }

                var jsonObj = new Dictionary<string, string>
                {
                    { "FullPath", path },
                    { "RelPath", relativeLocalPath },
                    { "Name", name },
                    { "Released", releaseDate.Ticks.ToString() },
                    { "Version", version },
                };
                return jsonObj;
            })
            .ToArray();

        var jsonFormatted = JsonConvert.SerializeObject(jsonVersionList, Newtonsoft.Json.Formatting.Indented);
        variables["vstoInfoJson"] = jsonFormatted;

        return variables;
    }
}