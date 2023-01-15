// <copyright file="Domain.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Entities;

public class Domain : BaseEntityWithIdAndDates
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public bool Shared { get; set; }

    public bool Disposable { get; set; }    
}

