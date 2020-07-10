// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

namespace SoftDeleteServices.ExampleConfigs
{
    public class MySoftDeleteIAccessor : ISoftDeleteAccess<ISoftDelete, bool>
    {

        public MySoftDeleteIAccessor(Guid currentUseId)
        {
            OtherFilters = new Dictionary<Type, Expression<Func<object, bool>>>
            {
                {typeof(IUserId), entity => ((IUserId) entity).UserId == currentUseId}
            };
        }

        public Expression<Func<ISoftDelete, bool>> GetSoftDeleteValue { get; } = entity => entity.SoftDeleted;
        public Action<ISoftDelete, bool> SetSoftDeleteValue { get; } = (entity, value) => { entity.SoftDeleted = value; };

        public Dictionary<Type, Expression<Func<object, bool>>> OtherFilters { get; } 

    }
}