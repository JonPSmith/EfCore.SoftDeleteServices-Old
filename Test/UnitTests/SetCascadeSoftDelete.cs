﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Concrete;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSetCascadeSoftDelete
    {
        private readonly ITestOutputHelper _output;
        private readonly Regex _selectMatchRegex = new Regex(@"SELECT "".""\.""EmployeeSoftCascadeId"",", RegexOptions.None);

        public TestSetCascadeSoftDelete(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                //VERIFY
                context.Employees.Count().ShouldEqual(11);
                context.Contracts.Count().ShouldEqual(9);
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
            }
        }

        [Fact]
        public void TestCascadeDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                //ATTEMPT
                context.Remove(ceo.WorksFromMe.First());
                context.SaveChanges();

                //VERIFY
                context.Employees.Count().ShouldEqual(4);
                context.Contracts.Count().ShouldEqual(3);
            }
        }

        [Fact]
        public void TestManualSoftDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                //ATTEMPT
                ceo.WorksFromMe.First().SoftDeleteLevel = 1;
                context.SaveChanges();

                //VERIFY
                context.Employees.Count().ShouldEqual(10);
            }
        }


        [Fact]
        public void TestCascadeSoftDeleteEmployeeSoftDelInfoOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var Info = service.SetCascadeSoftDelete(ceo.WorksFromMe.First());

                //VERIFY
                //EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                Info.NumFound.ShouldEqual(7 + 6);
                Info.ToString().ShouldEqual("You have soft deleted an entity and its 12 dependents");
            }
        }

        [Fact]
        public void TestCascadeSoftDeleteEmployeeSoftDelOneToOneOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var Info = service.SetCascadeSoftDelete(ceo.WorksFromMe.First().Contract);

                //VERIFY
                Info.NumFound.ShouldEqual(0);
                Info.ToString().ShouldEqual("You cannot soft delete a one-to-one relationship");
            }
        }

        [Fact]
        public void TestCascadeSoftDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var numSoftDeleted = service.SetCascadeSoftDelete(ceo.WorksFromMe.First()).NumFound;

                //VERIFY
                //EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                numSoftDeleted.ShouldEqual(7 + 6);
                context.Employees.Count().ShouldEqual(4);
                context.Employees.IgnoreQueryFilters().Count().ShouldEqual(11);
                context.Employees.IgnoreQueryFilters().Select(x => x.SoftDeleteLevel).Where(x => x > 0).ToArray()
                    .ShouldEqual(new byte[] { 1, 2, 2, 3, 3, 3, 3 });
                context.Contracts.Count().ShouldEqual(3);
                context.Contracts.IgnoreQueryFilters().Count().ShouldEqual(9);
                context.Employees.IgnoreQueryFilters().Select(x => x.Contract).Where(x => x != null)
                    .Select(x => x.SoftDeleteLevel).Where(x => x > 0).ToArray()
                    .ShouldEqual(new byte[] { 2, 3, 3, 4, 4, 4 });
            }
        }

        [Theory]
        [InlineData(false, 4)]
        [InlineData(true, 7)]
        public void TestCascadeSoftDeleteEmployeeSoftDelWithLoggingOk(bool readEveryTime, int selectCount)
        {
            //SETUP
            var logs = new List<string>();
            var options = SqliteInMemory.CreateOptionsWithLogging<SoftDelDbContext>(log => logs.Add(log.DecodeMessage()));
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                logs.Clear();
                var numSoftDeleted = service.SetCascadeSoftDelete(ceo.WorksFromMe.First(), readEveryTime).NumFound;

                //VERIFY
                logs.Count(x =>  _selectMatchRegex.IsMatch(x)).ShouldEqual(selectCount);
                numSoftDeleted.ShouldEqual(7 + 6);
                context.Employees.Count().ShouldEqual(4);
                context.Employees.IgnoreQueryFilters().Count().ShouldEqual(11);
                context.Employees.IgnoreQueryFilters().Select(x => x.SoftDeleteLevel).Where(x => x > 0).ToArray()
                    .ShouldEqual(new byte[] { 1, 2, 2, 3, 3, 3, 3 });
                context.Contracts.Count().ShouldEqual(3);
                context.Contracts.IgnoreQueryFilters().Count().ShouldEqual(9);
                context.Employees.IgnoreQueryFilters().Select(x => x.Contract).Where(x => x != null)
                    .Select(x => x.SoftDeleteLevel).Where(x => x > 0).ToArray()
                    .ShouldEqual(new byte[] { 2, 3, 3, 4, 4, 4 });
            }
        }

        [Fact]
        public void TestCascadeSoftDeleteExistingSoftDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var preNumSoftDeleted = service.SetCascadeSoftDelete(ceo.WorksFromMe.First().WorksFromMe.First()).NumFound;
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);

                //ATTEMPT
                var numSoftDeleted = service.SetCascadeSoftDelete(ceo.WorksFromMe.First()).NumFound;

                //VERIFY
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                preNumSoftDeleted.ShouldEqual(3 + 3);
                numSoftDeleted.ShouldEqual(4 + 3);
                context.Employees.Count().ShouldEqual(4);
                context.Employees.IgnoreQueryFilters().Count().ShouldEqual(11);
                context.Employees.IgnoreQueryFilters().Select(x => x.SoftDeleteLevel).Where(x => x > 0).ToArray()
                    .ShouldEqual(new byte[] { 1, 1, 2, 2, 2, 3, 3 });
                context.Contracts.Count().ShouldEqual(3);
                context.Contracts.IgnoreQueryFilters().Count().ShouldEqual(9);
                context.Employees.IgnoreQueryFilters().Select(x => x.Contract).Where(x => x != null)
                    .Select(x => x.SoftDeleteLevel).Where(x => x > 0)
                    .ToArray().ShouldEqual(new byte[] { 2, 2, 3, 3, 3, 4 });
            }
        }

        [Fact]
        public void TestCascadeSoftDeleteTwoLevelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var numInnerSoftDelete = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "ProjectManager1")).NumFound;
                numInnerSoftDelete.ShouldEqual(3 + 3);
                var numOuterSoftDelete = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).NumFound;

                //VERIFY
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                numOuterSoftDelete.ShouldEqual(4 + 3);
                context.Employees.Count().ShouldEqual(4);
                context.Employees.IgnoreQueryFilters().Count().ShouldEqual(11);
                context.Employees.IgnoreQueryFilters().Select(x => x.SoftDeleteLevel).Where(x => x > 0)
                    //.ToList().ForEach(x => _output.WriteLine(x.ToString()));
                    .ToArray().ShouldEqual(new byte[] { 1, 1, 2, 2,2, 3,3 });
                context.Contracts.Count().ShouldEqual(3);
                context.Contracts.IgnoreQueryFilters().Count().ShouldEqual(9);
                context.Employees.IgnoreQueryFilters().Select(x => x.Contract).Where(x => x != null)
                    .Select(x => x.SoftDeleteLevel).Where(x => x > 0)
                    //.ToList().ForEach(x => _output.WriteLine(x.ToString()));
                    .ToArray().ShouldEqual(new byte[] { 2, 2, 3, 3, 3, 4 });
            }
        }

        [Fact]
        public void TestCircularLoopCascadeSoftDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);
                var devEntry = context.Employees.Single(x => x.Name == "dev1a");
                devEntry.WorksFromMe = new List<EmployeeSoftCascade>{ devEntry.Manager.Manager};

                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).NumFound;

                //VERIFY
                numSoftDeleted.ShouldEqual(7+6);
                
            }
        }

        //---------------------------------------------------------
        //SetCascadeSoftDelete disconnected tests

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestDisconnectedCascadeSoftDeleteEmployeeSoftDelOk(bool readEveryTime)
        {
            //SETUP
            var logs = new List<string>();
            var options = SqliteInMemory.CreateOptionsWithLogging<SoftDelDbContext>(log => logs.Add(log.DecodeMessage()));
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                EmployeeSoftCascade.SeedEmployeeSoftDel(context);
            }
            using (var context = new SoftDelDbContext(options))
            {
                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                logs.Clear();
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO"), readEveryTime).NumFound;

                //VERIFY
                logs.Count(x => _selectMatchRegex.IsMatch(x)).ShouldEqual(7+7);
                numSoftDeleted.ShouldEqual(7+6);
                context.Employees.Count().ShouldEqual(4);
                context.Employees.IgnoreQueryFilters().Count().ShouldEqual(11);
                context.Contracts.Count().ShouldEqual(3);
                context.Contracts.IgnoreQueryFilters().Count().ShouldEqual(9);
            }
        }


    }
}