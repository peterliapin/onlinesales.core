// <copyright file="DictionaryExtensions.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Interfaces;

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
        Dictionary<string, string> convertedCollection = new Dictionary<string, string>();

        foreach (var item in targetDictionary)
        {
            convertedCollection.Add($"{prefix}{item.Key}{postfix}", item.Value);
        }

        return convertedCollection;
    }
}