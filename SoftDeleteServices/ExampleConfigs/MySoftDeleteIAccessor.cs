// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

namespace SoftDeleteServices.ExampleConfigs
{
    public class MySoftDeleteAccessor : ISoftDeleteAccess<ISoftDelete>
    {

        public MySoftDeleteAccessor(Guid currentUseId)
        {
            OtherFilters[typeof(IUserId)] = entity => ((IUserId) entity).UserId == currentUseId;
        }

        public ISoftDelete CurrentEntity { get; set; }
        public bool IsSoftDelete => CurrentEntity != null;
        public bool GetSoftDeleteValue => CurrentEntity.SoftDeleted;
        public Action<bool> SetSoftDeleteValue => input => CurrentEntity.SoftDeleted = input;

        public Expression<Func<ISoftDelete, bool>> FindSoftDeletedItems = entity => entity.SoftDeleted;

        public Dictionary<Type, Expression<Func<object, bool>>> OtherFilters =
            new Dictionary<Type, Expression<Func<object, bool>>>();

    }
}