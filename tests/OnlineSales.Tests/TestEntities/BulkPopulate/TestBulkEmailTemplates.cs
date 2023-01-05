// <copyright file="TestBulkEmailTemplates.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkEmailTemplates : EmailTemplate
    {
        public List<EmailTemplate> GenerateBulk(int groupId, int count)
        {
            var fakeCustomer = new Faker<EmailTemplate>().
                RuleFor(c => c.Name, f => f.Name.FirstName()).
                RuleFor(c => c.GroupId, groupId).
                RuleFor(c => c.Subject, f => f.Random.AlphaNumeric(10)).
                RuleFor(c => c.BodyTemplate, f => f.Name.LastName()).
                RuleFor(c => c.FromEmail, f => f.Internet.Email()).
                RuleFor(c => c.FromName, f => f.Name.FirstName()).
                RuleFor(c => c.Language, "en");                

            return fakeCustomer.Generate(count);
        }
    }
}
