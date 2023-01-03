// <copyright file="TestBulkPosts.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Bogus;
using OnlineSales.Entities;

namespace OnlineSales.Tests.TestEntities.BulkPopulate
{
    public class TestBulkPosts : Post
    {
        public List<Post> GenerateBulk(int count)
        {
            var fakeCustomer = new Faker<Post>().
                RuleFor(c => c.Slug, "Test Slug " + new Random().Next(0, 50000).ToString() + " - " + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString()).
                RuleFor(c => c.Template, f => f.Name.FirstName()).
                RuleFor(c => c.Author, f => f.Name.LastName()).
                RuleFor(c => c.Title, f => f.Random.Word()).
                RuleFor(c => c.Description, f => f.Random.AlphaNumeric(20));
                
            return fakeCustomer.Generate(count);
        }
    }
}
