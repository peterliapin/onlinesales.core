// <copyright file="ITestMigrationService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.Interfaces
{
    public interface ITestMigrationService
    {
        (bool, string) MigrateUpToAndCheck(string name);
    }
}