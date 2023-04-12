// <copyright file="IDomainService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IDomainService
    {
        public Task Verify(Domain domain);

        public Domain CreateDomain(string name, string? source = null);

        public void EnrichWithFreeAndDisposable(List<Domain> domains);

        public string GetDomainNameByUrl(string url);

        public string GetDomainNameByEmail(string email);
    }
}