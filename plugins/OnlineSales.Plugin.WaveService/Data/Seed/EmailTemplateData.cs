// <copyright file="EmailTemplateData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Entities;

namespace OnlineSales.Plugin.WaveService.Data.Seed;

public class EmailTemplateData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTemplate>().HasData(
            new EmailTemplate { GroupId = 1, Language = "ru", Name = "WA_Email_SendFile", Subject = "Download PDF", FromEmail = "support@waveservice.ru", FromName = "WaveService.ru Support", BodyTemplate = ReadResource("SendFile.html") });
    }

    private static string ReadResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

        if (resourcePath is null)
        {
            return string.Empty;
        }

        using (Stream stream = assembly!.GetManifestResourceStream(resourcePath !) !)
        {
            using (StreamReader reader = new StreamReader(stream!))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
