// <copyright file="IVariableService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IVariableService
    {
        public Dictionary<string, string> GetContextVariables(string contextKey, string? language);
    }
}