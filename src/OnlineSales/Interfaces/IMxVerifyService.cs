// <copyright file="IMxVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Services;

namespace OnlineSales.Interfaces;

public interface IMxVerifyService
{
    Task<bool> Verify(string mxValue);

    Task<MxCheckResult[]> BulkVerify(string mxValue, params string[] emails);
}