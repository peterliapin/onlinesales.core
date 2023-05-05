// <copyright file="TestData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public static class TestData
{
    public static List<T> Generate<T>(int count, params object[] args)
    {
        return GenerateAndPopulateAttributes<T>(count, null, args);
    }

    public static T Generate<T>(string uid = "", params object[] args)
    {
        return GenerateAndPopulateAttributes<T>(uid, null, args);
    }

    public static List<T> GenerateAndPopulateAttributes<T>(int count, Action<T>? populateAttributes = null, params object[] args)
    {
        var list = new List<T>();

        for (var i = 0; i < count; i++)
        {
            var uid = i.ToString();
            var instance = GenerateAndPopulateAttributes<T>(uid, populateAttributes, args);
            list.Add(instance);
        }

        return list;
    }

    public static T GenerateAndPopulateAttributes<T>(string uid = "", Action<T>? populateAttributes = null, params object[] args)
    {
        var argsList = new List<object>();
        argsList.Add(uid);
        argsList.AddRange(args);

        var instance = (T)Activator.CreateInstance(typeof(T), argsList.ToArray())!;

        if (populateAttributes != null)
        {
            populateAttributes(instance);
        }

        return instance;
    }
}
