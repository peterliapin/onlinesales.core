// <copyright file="ElasticMigration.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OnlineSales.Elastic;

[Table("migration")]
public class ElasticMigration
{
    public ElasticMigration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

        ProductVersion = fileVersionInfo.ProductVersion!;

        var elasticMigrationAttribute = GetType().GetCustomAttributes(typeof(ElasticMigrationAttribute), true).FirstOrDefault() as ElasticMigrationAttribute;

        if (elasticMigrationAttribute != null)
        {
            MigrationId = elasticMigrationAttribute.Id;
        }
    }

    public string MigrationId { get; set; } = string.Empty;

    public string ProductVersion { get; set; }

    public virtual async Task Up(ElasticDbContext context)
    {
        await Task.CompletedTask;

        throw new NotImplementedException();
    }
}

public class ElasticMigrationAttribute : Attribute
{
    public ElasticMigrationAttribute(string id)
    {
        Id = id;
    }

    public string Id { get; set; }
}