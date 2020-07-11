// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

[assembly: InternalsVisibleTo("Test")]

namespace SoftDeleteServices.Concrete.Internal
{
    internal class ExpressionBuilder<TInterface>
        where TInterface : class
    {
        private readonly SoftDeleteConfiguration<TInterface, bool> _config;

        public ExpressionBuilder(SoftDeleteConfiguration<TInterface, bool> config)
        {
            _config = config;
        }

        public Expression<Func<TEntity, bool>> FormFilterSingleSoftDelete<TEntity>()
            where TEntity : class, TInterface
        {
            var parameter = Expression.Parameter(typeof(TEntity), _config.GetSoftDeleteValue.Parameters.Single().Name);
            var left = Expression.Invoke(_config.GetSoftDeleteValue, parameter);
            var right = Expression.Constant(true);
            var result = Expression.Equal(left, right);

            if (!_config.OtherFilters.Any(x => x.Key.IsAssignableFrom(typeof(TEntity))))
                //no other filters to add, so go with the single one
                return Expression.Lambda<Func<TEntity, bool>>(result, parameter);

            foreach (var otherFilterType in _config.OtherFilters.Keys)
            {
                if (otherFilterType.IsAssignableFrom(typeof(TEntity)))
                {
                    var specificFilter = _config.OtherFilters[otherFilterType];
                    if (specificFilter.Parameters.Single().Name != _config.GetSoftDeleteValue.Parameters.Single().Name)
                        throw new InvalidOperationException(
                            $"The filter parameter for {otherFilterType.Name} must must match the " +
                            $"{nameof(_config.GetSoftDeleteValue)}, i.e. {_config.GetSoftDeleteValue.Parameters.Single().Name}.");
                    result = Expression.AndAlso(result,
                        Expression.Invoke(_config.OtherFilters[otherFilterType], parameter));
                }
            }

            return Expression.Lambda<Func<TEntity, bool>>(result, parameter);
        }
    }
}