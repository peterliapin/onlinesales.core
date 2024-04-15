// <copyright file="DateTimeHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Helpers;

public static class DateTimeHelper
{
    public static DateTime GetDateTime(double timestamp)
    {
        return DateTime.UnixEpoch.AddSeconds(timestamp).ToUniversalTime();
    }

    public static double GetTimeStamp(DateTime dateTime)
    {
        return (int)dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}