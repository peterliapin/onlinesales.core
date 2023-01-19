// <copyright file="IDomainService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces;

public interface IDomainService
{
    Task Verify(Domain domain);

    Task VerifyHttp(Domain domain);

    Task VerifyDns(Domain domain);
}