// <copyright file="TestAccount.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities
{
    public class TestAccount : AccountCreateDto
    {
        public TestAccount(string uid = "")
        {
            Name = $"Wave Access {uid}";
            EmployeesRange = "55";
            CountryCode = "LK";
            SocialMedia = new Dictionary<string, string>()
            {
                { "Facebook", "https://fb.com/waveaccess" },
                { "Instagram", "https://www.instagram.com//waveaccess" },
            };

            Tags = new string[] { "Information technology", "App Development" };
        }
    }
}
