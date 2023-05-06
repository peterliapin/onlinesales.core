// <copyright file="AccountsTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Tests;

public class AccountsTests : SimpleTableTests<Account, TestAccount, AccountUpdateDto, ISaveService<Account>>
{
    public AccountsTests()
        : base("/api/accounts")
    {
    }

    protected override AccountUpdateDto UpdateItem(TestAccount to)
    {
        var from = new AccountUpdateDto();
        to.Name = from.Name = to.Name + "Updated";
        return from;
    }
}