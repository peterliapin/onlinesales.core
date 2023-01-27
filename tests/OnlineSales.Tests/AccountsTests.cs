// <copyright file="AccountsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

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

        public override async Task CascadeDeleteTest()
        {
            var fkItem = await CreateFKItem();

            var fkItemId = fkItem.Item1;

            int numberOfItems = 3;

            string[] itemsUrls = new string[numberOfItems];

            for (var i = 0; i < numberOfItems; ++i)
            {
                var testItem = await CreateItem(i.ToString(), fkItemId);

                itemsUrls[i] = testItem.Item2;
            }

            await DeleteTest(fkItem.Item2, HttpStatusCode.UnprocessableEntity);
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
