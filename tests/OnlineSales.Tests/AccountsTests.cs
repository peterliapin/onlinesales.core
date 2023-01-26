// <copyright file="AccountsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq;
using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests
{
    public class AccountsTests : TableWithFKTests<Account, TestAccount, AccountUpdateDto>
    {
        public AccountsTests()
            : base("/api/account")
        {
        }

        protected override async Task<(int, string)> CreateFKItem()
        {
            var contactCreate = new TestDomain();

            var domainUrl = await PostTest("/api/domains", contactCreate);

            var domain = await GetTest<Domain>(domainUrl);

            domain.Should().NotBeNull();

            return (domain!.Id, domainUrl);
        }

        protected override async Task<(TestAccount, string)> CreateItem(string uid, int fkId)
        {
            var testAccount = new TestAccount(uid, fkId);

            var newUrl = await PostTest(itemsUrl, testAccount);

            return (testAccount, newUrl);
        }

        protected override AccountUpdateDto UpdateItem(TestAccount createdItem)
        {
            var from = new AccountUpdateDto();
            createdItem.Name = from.Name = createdItem.Name + " Updated";
            return from;
        }
    }
}
