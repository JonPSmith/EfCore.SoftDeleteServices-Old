// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Configuration;
using SoftDeleteServices.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSoftDeleteConfiguration
    {
        private readonly ITestOutputHelper _output;

        public TestSoftDeleteConfiguration(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void TestCanFilterUsingAccessorOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = new BookSoftDel { Title = "test", SoftDeleted = true};
                context.Add(book);
                context.SaveChanges();

                var access = new MySoftDeleteAccessor(Guid.Empty);

                //ATTEMPT
                var query = context.Books.IgnoreQueryFilters().Where(access.FindSoftDeletedItems).Cast<BookSoftDel>()
                    .Select(x => x.Title.Length);
                var result = query.ToList();

                //VERIFY
                _output.WriteLine(query.ToQueryString());
                result.Count.ShouldEqual(1);
            }
        }
    }
}