// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfCode;
using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

namespace SoftDeleteServices.ExampleConfigs
{
    public class ConfigISoftDeleteWithUserId : SoftDeleteConfiguration<ISingleSoftDelete, bool>
    {

        public ConfigISoftDeleteWithUserId(SoftDelDbContext content)
        {
            GetSoftDeleteValue = entity => entity.SoftDeleted;
            SetSoftDeleteValue = (entity, value) => { entity.SoftDeleted = value; };
            OtherFilters.Add(typeof(IUserId), entity => ((IUserId)entity).UserId == content.UserId);
        }
    }
}