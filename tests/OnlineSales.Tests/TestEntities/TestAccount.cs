// <copyright file="TestAccount.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities
{
    public class TestAccount : AccountCreateDto
    {
        public TestAccount(string uid = "", int domainId = 0)
        {
            Name = $"Wave Access {uid}";
            DomainId = domainId;
            EmployeesRate = "55";
            Country = "LK";
            SocialMedia = new Dictionary<string, string>()
            {
                { "Facebook", "https://fb.com/waveaccess" },
                { "Instagram", "https://www.instagram.com//waveaccess" },
            };

            Tags = new string[] { "Information technology", "App Development" };
        }
    }
}
