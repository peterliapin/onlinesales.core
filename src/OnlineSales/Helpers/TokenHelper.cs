// <copyright file="TokenHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Helpers;

public static class TokenHelper
{
    public static string ReplaceTokensFromVariables(Dictionary<string, string> variables, string stringToReplace)
    {
        foreach (var variable in variables)
        {
            stringToReplace = stringToReplace.Replace(variable.Key, variable.Value);
        }

        return stringToReplace;
    }
}