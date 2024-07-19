// <copyright file="UnsubscribeDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using CsvHelper.Configuration.Attributes;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public class UnsubscribeDto
{
    public int ContactId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
}

public class UnsubscribeDetailsDto : UnsubscribeDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UnsubscribeImportDto : BaseImportDtoWithIdAndSource
{
    private string contactEmail = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public int ContactId { get; set; }

    [Optional]
    [SurrogateForeignKey(typeof(Contact), "Email", "ContactId")]
    public string ContactEmail
    {
        get
        {
            return contactEmail;
        }

        set
        {
            contactEmail = value.ToLower();
        }
    }

    [Optional]
    public DateTime? CreatedAt { get; set; }
}