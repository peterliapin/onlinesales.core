// <copyright file="DateTimeHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace OnlineSales.Helpers;

public static class DateTimeHelper
{
    private static readonly DateTime UnixTimestampZeroDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime GetDateTime(double timestamp)
    {
        return UnixTimestampZeroDate.AddSeconds(timestamp).ToUniversalTime();
    }

    public static double GetTimeStamp(DateTime dateTime)
    {
        return (int)dateTime.Subtract(UnixTimestampZeroDate).TotalSeconds;
    }
}

