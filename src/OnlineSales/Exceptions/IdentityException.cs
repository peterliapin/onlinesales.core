// <copyright file="IdentityException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Identity;

namespace OnlineSales.Exceptions;
public class IdentityException : Exception
{
    public IdentityException(IEnumerable<IdentityError> errors)
    {
        var builder = new StringBuilder();

        foreach (var error in errors)
        {
            builder.AppendLine($"Code: {error.Code} Description: {error.Description}");
        }

        this.ErrorMessage = builder.ToString();
    }

    public IdentityException(IdentityError error)
    {
        this.ErrorMessage = $"Code: {error.Code} Description: {error.Description}";
    }

    public IdentityException(string error)
    {
        this.ErrorMessage = error;
    }

    public string ErrorMessage { get; set; }
}
