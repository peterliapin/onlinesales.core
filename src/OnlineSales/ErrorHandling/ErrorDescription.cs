// <copyright file="ErrorDescription.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;

namespace OnlineSales.ErrorHandling
{
    public struct ErrorDescription
    {
        public readonly string Code;

        public readonly string Title;

        public readonly string? Details;

        public ErrorDescription(string code, string title, string? details = null)
        {
            Code = code;
            Title = title;
            Details = details;
        }
    }
}