// <copyright file="IVariablesService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IVariablesService
    {
        public Dictionary<string, string> GetVariables(string language);

        public string[] GetProjectVersionsList(string project, string version, string language);
    }
}