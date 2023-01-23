// <copyright file="EmailDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
namespace OnlineSales.DTOs;
public class EmailVerifyDetailsDto : DomainDetailsDto
{
    public string EmailAddress { get; set; } = string.Empty;
}

public class EmailVerifyInfoDto
{
    public string EmailAddress { get; set; } = string.Empty;

    public string FreeCheck { get; set; } = string.Empty;

    public string DisposableCheck { get; set; } = string.Empty;

    public string CatchAllCheck { get; set; } = string.Empty;
}
