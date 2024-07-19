// <copyright file="ChangeLogMigrationException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class ChangeLogMigrationException : Exception
{
    public ChangeLogMigrationException(string message)
        : base(message)
    {
    }
}