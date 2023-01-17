// <copyright file="SupportChangeLogAttribute.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SupportChangeLogAttribute : Attribute
{
}