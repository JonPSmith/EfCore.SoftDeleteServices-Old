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
    public class TestHardDeleteSoftDelete
    {

        [Fact]
        public void TestHardDeleteSoftDeletedEntryOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            var book = setupContext.AddBookWithReviewToDb();
            var setupConfig = new ConfigSoftDeleteWithUserId(setupContext);
            var setupService = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, setupConfig);
            var setupStatus = setupService.SetSoftDelete(book);
            setupStatus.IsValid.ShouldBeTrue(setupStatus.GetAllErrors());

            using var attemptContext = new SingleSoftDelDbContext(options);
            var attemptConfig = new ConfigSoftDeleteWithUserId(attemptContext);
            var attemptService = new SingleSoftDeleteService<ISingleSoftDelete>(attemptContext, attemptConfig);
            //ATTEMPT
            var attemptStatus = attemptService.HardDeleteSoftDeletedEntry(attemptContext.Books.IgnoreQueryFilters().Single());
            //VERIFY
            attemptStatus.IsValid.ShouldBeTrue(attemptStatus.GetAllErrors());
            attemptStatus.Result.ShouldEqual(1);

            using var testContext = new SingleSoftDelDbContext(options);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(0);
        }

        [Fact]
        public void TestHardDeleteSoftDeletedEntryNoCallSaveChangesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            
            using var setupContext = new SingleSoftDelDbContext(options);            
            setupContext.Database.EnsureCreated();
            var book = setupContext.AddBookWithReviewToDb();
            var setupConfig = new ConfigSoftDeleteWithUserId(setupContext);
            var setupService = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, setupConfig);
            var setupStatus = setupService.SetSoftDelete(book);
            setupStatus.IsValid.ShouldBeTrue(setupStatus.GetAllErrors());

            using var testContext = new SingleSoftDelDbContext(options);
            var testConfig = new ConfigSoftDeleteWithUserId(testContext);
            var testService = new SingleSoftDeleteService<ISingleSoftDelete>(testContext, testConfig);
            //ATTEMPT
            var testStatus = testService.HardDeleteSoftDeletedEntry(testContext.Books.IgnoreQueryFilters().Single(), false);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            testContext.SaveChanges();
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(0);
            //VERIFY
            testStatus.IsValid.ShouldBeTrue(testStatus.GetAllErrors());
            testStatus.Result.ShouldEqual(1);
        }

        [Fact]
        public void TestHardDeleteViaKeysOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            
            using var setupContext = new SingleSoftDelDbContext(options);
            setupContext.Database.EnsureCreated();
            var book = setupContext.AddBookWithReviewToDb();

            var setupConfig = new ConfigSoftDeleteWithUserId(setupContext);
            var setupService = new SingleSoftDeleteService<ISingleSoftDelete>(setupContext, setupConfig);
            var setupStatus = setupService.SetSoftDelete(book);
            setupStatus.IsValid.ShouldBeTrue(setupStatus.GetAllErrors());

            using var attemptContext = new SingleSoftDelDbContext(options);            
            var attemptConfig = new ConfigSoftDeleteWithUserId(attemptContext);
            var attemptService = new SingleSoftDeleteService<ISingleSoftDelete>(attemptContext, attemptConfig);
            //ATTEMPT
            var attemptStatus = attemptService.HardDeleteViaKeys<Book>(attemptContext.Books.IgnoreQueryFilters().Single().Id);
            //VERIFY
            attemptStatus.IsValid.ShouldBeTrue(attemptStatus.GetAllErrors());
            attemptStatus.Result.ShouldEqual(1);
            
            using var testContext = new SingleSoftDelDbContext(options);
            testContext.Books.IgnoreQueryFilters().Count().ShouldEqual(0);
        }

        [Fact]
        public void TestHardDeleteViaKeysNotFoundOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            using var context = new SingleSoftDelDbContext(options);
            context.Database.EnsureCreated();
            var book = context.AddBookWithReviewToDb();

            var config = new ConfigSoftDeleteWithUserId(context);
            var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
            var status1 = service.SetSoftDelete(book);
            status1.IsValid.ShouldBeTrue(status1.GetAllErrors());

            //ATTEMPT
            var status = service.HardDeleteViaKeys<Book>(234);

            //VERIFY
            status.IsValid.ShouldBeFalse(status.GetAllErrors());
            status.GetAllErrors().ShouldEqual("Could not find the entry you ask for.");
        }

    }
}