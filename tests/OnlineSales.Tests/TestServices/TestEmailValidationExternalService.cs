// <copyright file="TestEmailValidationExternalService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests.TestServices;

public class TestEmailValidationExternalService : IEmailValidationExternalService
{
    public Task<EmailVerifyInfoDto> Validate(string email)
    {
        var emailVerifyInfo = new EmailVerifyInfoDto()
        {
            EmailAddress = email,
            CatchAllCheck = true,
            DisposableCheck = false,
            FreeCheck = false,
        };

        return Task.FromResult(emailVerifyInfo);
    }
}