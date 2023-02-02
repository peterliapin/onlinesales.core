// <copyright file="AccountsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using FluentAssertions;
using OnlineSales.DTOs;
using OnlineSales.Entities;

namespace OnlineSales.Tests
{
    public class AccountsTests : SimpleTableTests<Account, TestAccount, AccountUpdateDto> 
    {
        public AccountsTests()
            : base("/api/account")
        {
        }

        protected override AccountUpdateDto UpdateItem(TestAccount to)
        {
            var from = new AccountUpdateDto();
            to.Name = from.Name = to.Name + "Updated";
            return from;
        }
    }
}
