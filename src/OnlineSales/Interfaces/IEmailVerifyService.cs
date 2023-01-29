// <copyright file="IEmailVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IEmailVerifyService
    {
        Task<Domain> Validate(string email);

        Task VerifyEmail(string email, Domain domainRecord);
    }
}
