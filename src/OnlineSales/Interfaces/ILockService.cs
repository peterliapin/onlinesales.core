// <copyright file="ILockService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces;

public interface ILockService
{
    ILockHolder Lock(string key);

    ILockHolder? TryLock(string key);
}

public interface ILockHolder
{
}