// <copyright file="NonEmptyStringAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Nest;
using Quartz.Util;

namespace OnlineSales.DataAnnotations;
public class NonEmptyStringAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var data = value as string;

        if (data != null && data.Trim().Length == 0)
        {
            var memberName = validationContext.MemberName ?? string.Empty;
            return new ValidationResult(string.Format("The {0} field is empty", memberName));
        }

        return ValidationResult.Success!;
    }
}