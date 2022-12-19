// <copyright file="VariableService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class VariableService : IVariableService
{
    private const string DefaultLanguage = "en";
    private readonly IEnumerable<IVariableProvider> variableProviders;

    public VariableService(IEnumerable<IVariableProvider> variableProviders)
    {
        this.variableProviders = variableProviders;
    }

    public Dictionary<string, string> GetContextVariables(string contextKey, string? language)
    {
        Dictionary<string, string> contextVariables = new Dictionary<string, string>();

        foreach (var variableProvider in this.variableProviders)
        {
            if (variableProvider.Key == contextKey && variableProvider.Language == (language is null ? DefaultLanguage : language))
            {
                variableProvider.GenerateVariables();
                contextVariables.AddRangeIfNotExists(variableProvider.Variables);
            }
        }

        return contextVariables;
    }
}
