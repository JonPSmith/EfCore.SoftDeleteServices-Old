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


        [Fact]
        public void TestBuildExpressionQueryOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                ParameterExpression parameter = Expression.Parameter(typeof(ICascadeSoftDelete), "entity");
                Expression<Func<ICascadeSoftDelete, byte>> expression = entity => entity.SoftDeleteLevel;

                //Expression left = Expression.Call(parameter, null);
                //Expression right = Expression.Constant(1, typeof(byte));
                //var finalExp = Expression.Equal(left, right);

                ////ATTEMPT
                //var query = context.Employees.IgnoreQueryFilters().Where(x => finalExp(x)).Cast<BookSoftDel>()
                //    .Select(x => x.Title.Length);
                //var result = query.ToList();

                ////VERIFY
                //_output.WriteLine(query.ToQueryString());
                //result.Count.ShouldEqual(1);
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

                Expression<Func<ICascadeSoftDelete, bool>> expression = entity => entity.SoftDeleteLevel == 0;
                var alter = new AlterConstant(1);
                var changedExp = alter.Modify(expression) as Expression<Func<ICascadeSoftDelete, bool>>;

                //ATTEMPT
                var query = context.Employees.IgnoreQueryFilters().Where(changedExp).Cast<EmployeeSoftCascade>()
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