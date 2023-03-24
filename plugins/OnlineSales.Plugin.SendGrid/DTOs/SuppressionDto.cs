// <copyright file="SuppressionDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.SendGrid.DTOs;

public class SuppressionDto
{
    private static readonly DateTime UnixTimestampZeroDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public double Created
    {
        get
        {
            return (int)CreatedAt.Subtract(UnixTimestampZeroDate).TotalSeconds;
        }

        set
        {
            CreatedAt = UnixTimestampZeroDate.AddSeconds(value);
        }
    }

    public DateTime CreatedAt { get; set; }

    public string Email { get; set; } = string.Empty;

    public virtual string GetReason()
    {
        return "Unsubsribed";
    }
}

public class BlockOrBounceDto : SuppressionDto
{
    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public override string GetReason()
    {
        return $"{Status} - {Reason}";
    }
}

public class SpamReportDto : SuppressionDto
{
    public string Ip { get; set; } = string.Empty;

    public override string GetReason()
    {
        return $"Reported As Spam";
    }
}