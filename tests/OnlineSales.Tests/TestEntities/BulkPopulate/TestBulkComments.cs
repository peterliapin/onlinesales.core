// <copyright file="TestBulkComments.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkComments : Comment
    {
        public List<Comment> GenerateBulk(int postId, int count)
        {
            var fakeCustomer = new Faker<Comment>().
                RuleFor(c => c.PostId, postId).
                RuleFor(c => c.AuthorEmail, f => f.Internet.Email()).
                RuleFor(c => c.AuthorName, f => f.Name.FirstName()).
                RuleFor(c => c.Content, f => f.Random.AlphaNumeric(10));

            return fakeCustomer.Generate(count);
        }
    }
}
