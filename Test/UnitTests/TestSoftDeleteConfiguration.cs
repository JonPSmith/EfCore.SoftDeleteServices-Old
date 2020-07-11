// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using DataLayer.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Configuration;
using SoftDeleteServices.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;

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

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);

                //ATTEMPT
                var getSoftValue = config.GetSoftDeleteValue.Invoke(book);
                getSoftValue.ShouldBeTrue();
                var query = context.Books.IgnoreQueryFilters().Where(config.GetSoftDeleteValue).Cast<BookSoftDel>()
                    .Select(x => x.Title.Length);
                var result = query.ToList();

                //VERIFY
                _output.WriteLine(query.ToQueryString());
                result.Count.ShouldEqual(1);
            }
        }



        [Fact]
        public void TestConvertFuncToQueryOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);
                ceo.SoftDeleteLevel = 1;
                ceo.WorksFromMe.First().SoftDeleteLevel = 1;
                context.SaveChanges();

                Expression<Func<ICascadeSoftDelete, byte>> expression = entity => entity.SoftDeleteLevel;

                var parameter = Expression.Parameter(typeof(ICascadeSoftDelete), expression.Parameters.Single().Name);
                var left = Expression.Invoke(expression,  parameter);
                var right = Expression.Constant((byte)1, typeof(byte));
                var equal = Expression.Equal(left, right);
                var dynamicFilter = Expression.Lambda<Func<ICascadeSoftDelete, bool>>(equal, parameter);

               //ATTEMPT
               var query = context.Employees.IgnoreQueryFilters()
                   .Where(dynamicFilter).Cast<EmployeeSoftCascade>()
                   .Select(x => x.Name);
                var result = query.ToList();

                //VERIFY
                _output.WriteLine(query.ToQueryString());
                result.Count.ShouldEqual(2);
            }
        }




        public class AlterConstant : ExpressionVisitor
        {
            private int _newConstant;

            public AlterConstant(int newConstant)
            {
                _newConstant = newConstant;
            }


            public Expression Modify(Expression expression)
            {
                return Visit(expression);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return Expression.Constant(_newConstant);
            }
        }
    }
}