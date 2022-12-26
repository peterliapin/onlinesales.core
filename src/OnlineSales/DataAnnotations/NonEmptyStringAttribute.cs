// <copyright file="NonEmptyStringAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DataAnnotations;
public class NonEmptyStringAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string data && data.Trim().Length == 0)
        {
            var memberName = validationContext.MemberName ?? string.Empty;
            return new ValidationResult($"The {memberName} field is empty");
        }

        return ValidationResult.Success!;
    }
}