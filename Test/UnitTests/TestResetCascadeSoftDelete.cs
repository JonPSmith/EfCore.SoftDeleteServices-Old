// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
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
    public class TestResetCascadeSoftDelete
    {
        private readonly ITestOutputHelper _output;

        public TestResetCascadeSoftDelete(ITestOutputHelper output)
        {
            _output = output;
        }

        //---------------------------------------------------------
        //reset 

        [Fact]
        public void TestResetCascadeSoftOfPreviousDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).Result;
                numSoftDeleted.ShouldEqual(7 + 6);
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);

                //ATTEMPT
                var status = service.ResetCascadeSoftDelete(context.Employees.IgnoreQueryFilters().Single(x => x.Name == "CTO"));

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(7 + 6);
                context.Employees.Count().ShouldEqual(11);
                context.Contracts.Count().ShouldEqual(9);
            }
        }

        [Fact]
        public void TestResetCascadeSoftOfPreviousDeleteInfo()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).Result;
                numSoftDeleted.ShouldEqual(7 + 6);
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);

                //ATTEMPT
                var status = service.ResetCascadeSoftDelete(context.Employees.IgnoreQueryFilters().Single(x => x.Name == "CTO"));

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(7 + 6);
                status.Message.ShouldEqual("You have recovered an entity and its 12 dependents");
            }
        }

        [Fact]
        public void TestResetCascadeSoftDeletePartialOfPreviousDeleteDoesNothingOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).Result;
                numSoftDeleted.ShouldEqual(7 + 6);
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);

                //ATTEMPT
                var status = service.ResetCascadeSoftDelete(context.Employees.IgnoreQueryFilters().Single(x => x.Name == "ProjectManager1"));

                //VERIFY
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                status.IsValid.ShouldBeFalse();
                status.GetAllErrors().ShouldEqual("This entry was soft deleted 1 level above here");
                status.Result.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestResetCascadeSoftDeleteTwoLevelSoftDeleteThenUndeleteTopOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var numInnerSoftDelete = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "ProjectManager1")).Result;
                numInnerSoftDelete.ShouldEqual(3 + 3);
                var numOuterSoftDelete = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).Result;
                numOuterSoftDelete.ShouldEqual(4 + 3);
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);

                //ATTEMPT
                var status = service.ResetCascadeSoftDelete(context.Employees.IgnoreQueryFilters().Single(x => x.Name == "CTO"));

                //VERIFY
                EmployeeSoftCascade.ShowHierarchical(ceo, x => _output.WriteLine(x), false);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(4 + 3);
                var cto = context.Employees.Include(x => x.WorksFromMe).Single(x => x.Name == "CTO");
                cto.WorksFromMe.Single(x => x.SoftDeleteLevel == 0).Name.ShouldEqual("ProjectManager2");
            }
        }

        //-------------------------------------------------------------
        //disconnected state

        [Fact]
        public void TestDisconnectedResetCascadeSoftDeleteEmployeeSoftDelOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var ceo = EmployeeSoftCascade.SeedEmployeeSoftDel(context);

                var service = new CascadeSoftDelService(context);
                var numSoftDeleted = service.SetCascadeSoftDelete(context.Employees.Single(x => x.Name == "CTO")).Result;
                numSoftDeleted.ShouldEqual(7+6);
            }

            using (var context = new SoftDelDbContext(options))
            {
                var service = new CascadeSoftDelService(context);

                //ATTEMPT
                var status = service.ResetCascadeSoftDelete(context.Employees.IgnoreQueryFilters().Single(x => x.Name == "CTO"));

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(7 + 6);
                context.Employees.Count().ShouldEqual(11);
                context.Contracts.Count().ShouldEqual(9);
            }
        }



    }
}