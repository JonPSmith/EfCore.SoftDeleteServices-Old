// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataLayer.EfCode
{
    public enum MyQueryFilterTypes { SingleSoftDelete, UserId, SingleSoftDeleteAndUserId }    

    public class QueryFilterAutoConfig                             
    {
        private readonly IUserId _userIdProvider;                             

        public QueryFilterAutoConfig(IUserId userIdProvider)                  
        {                                                          
            _userIdProvider = userIdProvider;                                      
        }

        public void SetQueryFilter(IMutableEntityType entityData,  
            MyQueryFilterTypes queryFilterType)                    
        {
            var methodName = $"Get{queryFilterType}Filter";        
            var methodToCall = this.GetType().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(entityData.ClrType);            
            var filter = methodToCall                              
                .Invoke(this, new object[] { });                   
            entityData.SetQueryFilter((LambdaExpression)filter);   
        }

        private LambdaExpression GetSingleSoftDeleteFilter<TEntity>()
            where TEntity : class, ISingleSoftDelete
        {
            Expression<Func<TEntity, bool>> filter = x => !x.SoftDeleted;
            return filter;
        }

        private LambdaExpression GetUserIdFilter<TEntity>()
            where TEntity : class, IUserId                 
        {                                                  
            Expression<Func<TEntity, bool>> filter = x => x.UserId == _userIdProvider.UserId;                  
            return filter;                                 
        }


        private LambdaExpression GetSingleSoftDeleteAndUserIdFilter<TEntity>()
            where TEntity : class, IUserId, ISingleSoftDelete
        {
            Expression<Func<TEntity, bool>> filter = x => x.UserId == _userIdProvider.UserId && !x.SoftDeleted;
            return filter;
        }

    }
}