// <copyright file="TestData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestEntities;

public static class TestData    
{
    public static List<T> Generate<T>(int count, params object[] args)
        where T : class
    {
        var list = new List<T>();

        for (var i = 0; i < count; i++)
        {
            var uid = i.ToString();
            var instance = Generate<T>(uid, args);
            list.Add(instance);
        }

        return list;
    }

    public static T Generate<T>(string uid = "", params object[] args)
    {
        var argsList = new List<object>();
        argsList.Add(uid);
        argsList.AddRange(args);

        return (T)Activator.CreateInstance(typeof(T), argsList.ToArray()) !;
    }
}

