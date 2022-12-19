// <copyright file="IVariableProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces;

public interface IVariableProvider
{
    public Dictionary<string, string> Variables { get; set; }

    public string Key { get; set; }

    public string Language { get; set; }

    public void GenerateVariables();
}
