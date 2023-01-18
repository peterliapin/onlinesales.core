// <copyright file="IDomainCheckService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IDomainCheckService
    {
        public Task HttpCheck(Domain d);

        public Task DnsCheck(Domain d);
    }
}