// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.Interfaces;
using DataLayer.SingleEfClasses;
using DataLayer.SingleEfCode;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Concrete;
using Test.EfHelpers;
using Test.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.SingleSoftDeleteTests
{
    public class TestResetSoftDeleteAndGetSoftDeleted
    {

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            var book = setupContext.AddBookWithReviewToDb();

            var config = new ConfigSoftDeleteWithUserId(setupContext);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, config);
            service.SetSoftDelete(book);

            //ATTEMPT
            var status = service.ResetSoftDelete(book);

            //VERIFY
            status.IsValid.ShouldBeTrue(status.GetAllErrors());
            status.Result.ShouldEqual(1);

            using var testContext = new SingleSoftDelDbContext(options);
            testContext.Books.Count().ShouldEqual(1);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
        }

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteNoCallSaveChangesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var context = new SingleSoftDelDbContext(options);
            context.Database.EnsureCreated();
            var book = context.AddBookWithReviewToDb();

            var config = new ConfigSoftDeleteWithUserId(context);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
            service.SetSoftDelete(book);

            //ATTEMPT
            var status = service.ResetSoftDelete(book, false);
            context.Books.Count().ShouldEqual(0);
            context.SaveChanges();
            context.Books.Count().ShouldEqual(1);

            //VERIFY
            status.IsValid.ShouldBeTrue(status.GetAllErrors());
            status.Result.ShouldEqual(1);
            context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
        }

        [Fact]
        public void TestSoftDeleteServiceDddResetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            setupContext.Database.EnsureCreated();
            var bookDdd = new BookDDD("Test");
            setupContext.Add(bookDdd);
            setupContext.SaveChanges();

            var config = new ConfigSoftDeleteDDD();
            var service = new SingleSoftDeleteService<ISingleSoftDeletedDDD>(setupContext, config);
            service.SetSoftDelete(bookDdd);

            //ATTEMPT
            var status = service.ResetSoftDelete(bookDdd);

            //VERIFY
            status.IsValid.ShouldBeTrue(status.GetAllErrors());
            status.Result.ShouldEqual(1);

            using var testContext = new SingleSoftDelDbContext(options);
            testContext.BookDdds.Count().ShouldEqual(1);
            testContext.BookDdds.IgnoreQueryFilters().Count().ShouldEqual(1);
        }

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteViaKeysOk()
        {
            //SETUP
            int bookId;
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            bookId = setupContext.AddBookWithReviewToDb().Id;

            var config = new ConfigSoftDeleteWithUserId(setupContext);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, config);
            var status1 = service.SetSoftDeleteViaKeys<Book>(bookId);
            status1.IsValid.ShouldBeTrue(status1.GetAllErrors());

            //ATTEMPT
            var status2 = service.ResetSoftDeleteViaKeys<Book>(bookId);

            //VERIFY
            status2.IsValid.ShouldBeTrue(status2.GetAllErrors());
            status2.Result.ShouldEqual(1);

            using var testContext = new SingleSoftDelDbContext(options);
            testContext.Books.Count().ShouldEqual(1);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
        }

        [Fact]
        public void TestHardDeleteViaKeysWithUserIdOk()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            int bookId;
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            var order1 = new Order
                { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser };
            var order2 = new Order
                { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
            var order3 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
            var order4 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = false, UserId = Guid.NewGuid() };
            setupContext.AddRange(order1, order2, order3, order4);
            setupContext.SaveChanges();
            bookId = order1.Id;
            
            using var testContext = new SingleSoftDelDbContext(options, currentUser);
            var config = new ConfigSoftDeleteWithUserId(testContext);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(testContext, config);

            //ATTEMPT
            var status = service.ResetSoftDeleteViaKeys<Order>(bookId);

            //VERIFY
            status.IsValid.ShouldBeTrue(status.GetAllErrors());
            status.Result.ShouldEqual(1);
            testContext.Orders.IgnoreQueryFilters().Count().ShouldEqual(4);
            testContext.Orders.Count().ShouldEqual(2);
        }

        [Fact]
        public void TestHardDeleteViaKeysWithWrongUserIdBad()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            int bookId;
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            var order1 = new Order
                { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser };
            var order2 = new Order
                { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
            var order3 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
            var order4 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = false, UserId = Guid.NewGuid() };
            setupContext.AddRange(order1, order2, order3, order4);
            setupContext.SaveChanges();
            bookId = order3.Id;
            
            using var testContext = new SingleSoftDelDbContext(options, currentUser);
            var config = new ConfigSoftDeleteWithUserId(testContext);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(testContext, config);

            //ATTEMPT
            var status = service.ResetSoftDeleteViaKeys<Order>(bookId);

            //VERIFY
            status.IsValid.ShouldBeFalse();
            status.GetAllErrors().ShouldEqual("Could not find the entry you ask for.");
            testContext.Orders.IgnoreQueryFilters().Count().ShouldEqual(4);
            testContext.Orders.Count().ShouldEqual(1);
        }

        //-------------------------------------------
        //GetSoftDeletedEntries 

        [Fact]
        public void TestSoftDeleteServiceGetSoftDeletedEntriesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);            
            setupContext.Database.EnsureCreated();
            var book1 = setupContext.AddBookWithReviewToDb("test1");
            var book2 = setupContext.AddBookWithReviewToDb("test2");

            var setupConfig = new ConfigSoftDeleteWithUserId(setupContext);
            var setupService = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, setupConfig);
            var status = setupService.SetSoftDelete(book1);
            status.IsValid.ShouldBeTrue(status.GetAllErrors());

            
            using var testContext = new SingleSoftDelDbContext(options);
            var testConfig = new ConfigSoftDeleteWithUserId(testContext);
            var testService = new SingleSoftDeleteService<ISingleSoftDelete>(testContext, testConfig);

            //ATTEMPT
            var softDelBooks = testService.GetSoftDeletedEntries<Book>().ToList();

            //VERIFY
            softDelBooks.Count.ShouldEqual(1);
            softDelBooks.Single().Title.ShouldEqual("test1");
            testContext.Books.Count().ShouldEqual(1);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(2);
        }

        [Fact]
        public void TestSoftDeleteServiceGetSoftDeletedEntriesWithUserIdOk()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var setupContext = new SingleSoftDelDbContext(options);            
            setupContext.Database.EnsureCreated();
            var order1 = new Order
                { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser};
            var order2 = new Order
                { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
            var order3 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
            var order4 = new Order
                { OrderRef = "Diff user Order", SoftDeleted = false, UserId = Guid.NewGuid() };
            setupContext.AddRange(order1, order2, order3, order4);
            setupContext.SaveChanges();

            using var testContext = new SingleSoftDelDbContext(options, currentUser);
            var config = new ConfigSoftDeleteWithUserId(testContext);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(testContext, config);

            //ATTEMPT
            var orders = service.GetSoftDeletedEntries<Order>().ToList();

            //VERIFY
            orders.Count.ShouldEqual(1);
            orders.Single(x => x.UserId == currentUser).OrderRef.ShouldEqual("Cur user Order, soft del");
            testContext.Orders.IgnoreQueryFilters().Count().ShouldEqual(4);
            var all = testContext.Orders.IgnoreQueryFilters().ToList();
            testContext.Orders.Count().ShouldEqual(1);
        }
    }
}