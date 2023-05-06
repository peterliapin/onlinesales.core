// <copyright file="VstoVariablesProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

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

        return variables;
    }
}