// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using DataLayer.Interfaces;
using DataLayer.SingleEfCode;
using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;
using SoftDeleteServices;
using SoftDeleteServices.Concrete;
using SoftDeleteServices.Configuration;
using Test.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;

namespace Test.UnitTests.OtherTests
{
    public class TestSoftDeleteDi
    {
        [Fact]
        public void TestRegisterServiceManuallyOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SingleSoftDelDbContext>();
            var context = new SingleSoftDelDbContext(options);
            
            //ATTEMPT
            var services = new ServiceCollection();
            services.AddScoped(x => context);
            services.AddSingleton<SoftDeleteConfiguration<ISingleSoftDelete, bool>, ConfigSoftDeleteWithUserId>();
            services.AddTransient<SingleSoftDeleteService<ISingleSoftDelete>>();
            var serviceProvider = services.BuildServiceProvider();

            //VERIFY
            var service1 = serviceProvider.GetRequiredService<SingleSoftDelDbContext>();
            var service2 = serviceProvider.GetRequiredService<SoftDeleteConfiguration<ISingleSoftDelete, bool>>();
            var service3 = serviceProvider.GetRequiredService<SingleSoftDeleteService<ISingleSoftDelete>>();
        }        
        
    }
}