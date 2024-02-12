// <copyright file="IEmailSchedulingService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Interfaces;

public interface IEmailSchedulingService
{
    Task<EmailSchedule?> FindByGroupAndLanguage(string groupName, string languageCode);

    void SetDBContext(PgDbContext pgDbContext);
}

