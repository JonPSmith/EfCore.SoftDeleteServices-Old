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
using SoftDeleteServices.Concrete.Internal;
using SoftDeleteServices.Configuration;
using Test.ExampleConfigs;
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
        public void TestExpressionBuilderFormFilterSingleSoftDeleteWithUserIdOk()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options, currentUser))
            {
                context.Database.EnsureCreated();
                var order1 = new OrderSingleSoftDelUserId
                    { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser };
                var order2 = new OrderSingleSoftDelUserId
                    { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
                var order3 = new OrderSingleSoftDelUserId
                    { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
                context.AddRange(order1, order2, order3);
                context.SaveChanges();

                var config = new ConfigISoftDeleteWithUserId(context);
                var builder = new ExpressionBuilder<ISingleSoftDelete>(config);

                //ATTEMPT
                var query = context.Orders.IgnoreQueryFilters().Where(builder.FilterToGetValueSingleSoftDeletedEntities<OrderSingleSoftDelUserId>())
                    .Select(x => x.OrderRef);
                var result = query.ToList();

                //VERIFY
                _output.WriteLine(query.ToQueryString());
                result.Count.ShouldEqual(1);
            }
        }
        [Fact]
        public void TestExpressionBuilderFormOtherFiltersOnlyWithUserIdOk()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options, currentUser))
            {
                context.Database.EnsureCreated();
                var order1 = new OrderSingleSoftDelUserId
                    { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser };
                var order2 = new OrderSingleSoftDelUserId
                    { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
                var order3 = new OrderSingleSoftDelUserId
                    { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
                context.AddRange(order1, order2, order3);
                context.SaveChanges();

                var config = new ConfigISoftDeleteWithUserId(context);
                var builder = new ExpressionBuilder<ISingleSoftDelete>(config);

                //ATTEMPT
                var query = context.Orders.IgnoreQueryFilters().Where(builder.FilterToGetValueSingleSoftDeletedEntities<OrderSingleSoftDelUserId>())
                    .Select(x => x.OrderRef);
                var result = query.ToList();

                //VERIFY
                _output.WriteLine(query.ToQueryString());
                result.Count.ShouldEqual(1);
            }
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

                var config = new ConfigISoftDeleteWithUserId(context);

                //ATTEMPT
                var getSoftValue = config.GetSoftDeleteValue.Compile().Invoke(book);
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
    }
}