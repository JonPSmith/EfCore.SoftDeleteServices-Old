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
using Test.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSingleSoftDeleteService
    {
        [Fact]
        public void TestAddBookWithReviewOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                AddBookWithReviewToDb(context);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                //VERIFY
                var book = context.Books.Include(x => x.Reviews).Single();
                book.Title.ShouldEqual("test");
                book.Reviews.ShouldNotBeNull();
                book.Reviews.Single().NumStars.ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDelete(book);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(0);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<Book>(book.Id);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(0);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysBadKeyType()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var ex = Assert.Throws<ArgumentException>(() => service.SetSoftDeleteViaKeys<Book>(book));

                //VERIFY
                ex.Message.ShouldEqual("Mismatch in keys: your provided key 1 (of 1) is of type Book but entity key's type is System.Int32 (Parameter 'keyValues')");
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysBadNumberOfKeys()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var ex = Assert.Throws<ArgumentException>(() => service.SetSoftDeleteViaKeys<Book>(1,2));

                //VERIFY
                ex.Message.ShouldEqual("Mismatch in keys: your provided 2 key(s) and the entity has 1 key(s) (Parameter 'keyValues')");
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysNotFoundBad()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<Book>(123);

                //VERIFY
                status.IsValid.ShouldBeFalse();
                status.GetAllErrors().ShouldEqual("Could not find the entry you ask for.");
                status.Result.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysNotFoundReturnsZero()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                var config = new ConfigSoftDeleteWithUserId(context) {NotFoundIsNotAnError = true};
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<Book>(123);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(0);
            }
        }

        //----------------------------------------
        //DDD Set

        [Fact]
        public void TestSoftDeleteServiceDddSetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var bookDdd = new BookDDD("Test");
                context.Add(bookDdd);
                context.SaveChanges();

                var config = new ConfigSoftDeleteDDD();
                var service = new SingleSoftDeleteService<ISingleSoftDeletedDDD>(context, config);

                //ATTEMPT
                var status = service.SetSoftDelete(bookDdd);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.BookDdds.Count().ShouldEqual(0);
                context.BookDdds.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDddDeleteViaKeysOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var bookDdd = new BookDDD("Test");
                context.Add(bookDdd);
                context.SaveChanges();

                var config = new ConfigSoftDeleteDDD();
                var service = new SingleSoftDeleteService<ISingleSoftDeletedDDD>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<BookDDD>(bookDdd.Id);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.BookDdds.Count().ShouldEqual(0);
                context.BookDdds.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }


        //----------------------------------------
        //RESET

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                service.SetSoftDelete(book);

                //ATTEMPT
                var status = service.ResetSoftDelete(book);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceDddResetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                context.Database.EnsureCreated();
                var bookDdd = new BookDDD("Test");
                context.Add(bookDdd);
                context.SaveChanges();

                var config = new ConfigSoftDeleteDDD();
                var service = new SingleSoftDeleteService<ISingleSoftDeletedDDD>(context, config);
                service.SetSoftDelete(bookDdd);

                //ATTEMPT
                var status = service.ResetSoftDelete(bookDdd);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.BookDdds.Count().ShouldEqual(1);
                context.BookDdds.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }
        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteViaKeysOk()
        {
            //SETUP
            int bookId;
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                bookId = AddBookWithReviewToDb(context).Id;

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                var status1 = service.SetSoftDeleteViaKeys<Book>(bookId);
                status1.IsValid.ShouldBeTrue(status1.GetAllErrors());

                //ATTEMPT
                var status2 = service.ResetSoftDeleteViaKeys<Book>(bookId);

                //VERIFY
                status2.IsValid.ShouldBeTrue(status2.GetAllErrors());
                status2.Result.ShouldEqual(1);
            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }



        [Fact]
        public void TestSoftDeleteServiceGetSoftDeletedEntriesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book1 = AddBookWithReviewToDb(context, "test1");
                var book2 = AddBookWithReviewToDb(context, "test2");

                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                var status = service.SetSoftDelete(book1);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());

            }
            using (var context = new SingleSoftDelDbContext(options))
            {
                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var softDelBooks = service.GetSoftDeletedEntries<Book>().ToList();

                //VERIFY
                softDelBooks.Count.ShouldEqual(1);
                softDelBooks.Single().Title.ShouldEqual("test1");
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(2);
            }
        }


        [Fact]
        public void TestSoftDeleteServiceGetSoftDeletedEntriesWithUserIdOk()
        {
            //SETUP
            var currentUser = Guid.NewGuid();
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using (var context = new SingleSoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var order1 = new Order
                    { OrderRef = "Cur user Order, soft del", SoftDeleted = true, UserId = currentUser};
                var order2 = new Order
                    { OrderRef = "Cur user Order", SoftDeleted = false, UserId = currentUser };
                var order3 = new Order
                    { OrderRef = "Diff user Order", SoftDeleted = true, UserId = Guid.NewGuid() };
                context.AddRange(order1, order2, order3);
                context.SaveChanges();
            }
            using (var context = new SingleSoftDelDbContext(options, currentUser))
            {
                var config = new ConfigSoftDeleteWithUserId(context);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var orders = service.GetSoftDeletedEntries<Order>().ToList();

                //VERIFY
                orders.Count.ShouldEqual(1);
                orders.Single().OrderRef.ShouldEqual("Cur user Order, soft del");
                context.Orders.IgnoreQueryFilters().Count().ShouldEqual(3);
                var all = context.Orders.IgnoreQueryFilters().ToList();
                context.Orders.Count().ShouldEqual(1);
            }
        }

        private static Book AddBookWithReviewToDb(SingleSoftDelDbContext context, string title = "test")
        {
            var book = new Book
                {Title = title, Reviews = new List<Review> {new Review {NumStars = 1}}};
            context.Add(book);
            context.SaveChanges();
            return book;
        }
    }
}