// <copyright file="TestData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Plugin.TestPlugin.Entities;

namespace OnlineSales.Plugin.TestPlugin.TestData;
public class ChangeLogMigrationsTestData
{
    public static readonly int NumberOfDeletedEntities = 3;

    public static readonly int NumberOfRecords = 10;

    public static readonly string AddedColumnName = "int_field";

    public static readonly int AddedColumnDefaultValue = 7;

    static ChangeLogMigrationsTestData()
    {
        InsertionData = new string[NumberOfRecords];
        for (int i = 0; i < NumberOfRecords; ++i)
        {
            InsertionData[i] = $"StringField{i}";
        }
    }

    public static string[] InsertionData { get; }
}