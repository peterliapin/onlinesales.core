// <copyright file="ISmsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface ISmsService
    {
        Task SendAsync(string recipient, string message);
    }
}