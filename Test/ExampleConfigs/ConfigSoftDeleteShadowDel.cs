// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.Interfaces;
using DataLayer.SingleEfCode;
using SoftDeleteServices.Configuration;

namespace Test.ExampleConfigs
{
    public class ConfigSoftDeleteShadowDel : SingleSoftDeleteConfiguration<IShadowSoftDelete>
    {

        public ConfigSoftDeleteShadowDel(SingleSoftDelDbContext context)
            : base(context)
        {
            GetSoftDeleteValue = entity => (bool)context.Entry(entity).Property("SoftDeleted").CurrentValue;
            SetSoftDeleteValue = (entity, value) => { context.Entry(entity).Property("SoftDeleted").CurrentValue = value; }; 
        }
    }
}