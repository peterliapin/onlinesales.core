// <copyright file="DictionaryExtensions.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Infrastructure;

public static class DictionaryExtensions
{
    public static void AddRangeIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> targetDictionary, IDictionary<TKey, TValue> sourceDictionary)
    {
        foreach (var item in sourceDictionary)
        {
            if (!targetDictionary.ContainsKey(item.Key))
            {
                targetDictionary.Add(item.Key, item.Value);
            }
        }
    }

    public static Dictionary<string, string> ConvertKeys(this Dictionary<string, string> targetDictionary, string prefix, string postfix)
    {
        return targetDictionary.ToDictionary(item => $"{prefix}{item.Key}{postfix}", item => item.Value);
    }
}