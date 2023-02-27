// <copyright file="ResponseHeaderNames.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Infrastructure;

/// <summary>
/// Response Headers custom names.
/// </summary>
public static class ResponseHeaderNames
{
    /// <summary>
    /// Gets header name for total count of records.
    /// </summary>
    public static string TotalCount => "x-total-count";

    public static string AccessControlExposeHeader => "Access-Control-Expose-Headers";
}
