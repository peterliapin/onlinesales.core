// <copyright file="TestBulkEmailGroups.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkEmailGroups : EmailGroup
    {
        public List<EmailGroup> GenerateBulk(int count)
        {
            var fakeCustomer = new Faker<EmailGroup>().
                RuleFor(c => c.Name, f => f.Name.FirstName()).
                RuleFor(c => c.Language, "en");
                
            return fakeCustomer.Generate(count);
        }
    }
}
