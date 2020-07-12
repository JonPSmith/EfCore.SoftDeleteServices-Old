// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using SoftDeleteServices.Configuration;
using static System.Linq.Expressions.Expression;

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

        /// <summary>
        /// This returns a where filter that returns all the valid entities that have been single soft deleted
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> FilterToGetValueSingleSoftDeletedEntities<TEntity>()
            where TEntity : class, TInterface
        {
            var parameter = Parameter(typeof(TEntity), _config.GetSoftDeleteValue.Parameters.Single().Name);
            var left = Invoke(_config.GetSoftDeleteValue, parameter);
            var right = Constant(true);
            var result = Equal(left, right);

            return AddOtherFilters<TEntity>(result, parameter, _config.OtherFilters);
        }

        /// <summary>
        /// This returns all the entities of this type that are valid, e.g. not filtered out by other parts of the Query filters
        /// Relies on the user filling in the OtherFilters part of the config
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> FormOtherFiltersOnly<TEntity>(Dictionary<Type, Expression<Func<object, bool>>> otherFilters)
        {
            var parameter = otherFilters.Values.Any()
                ? Parameter(typeof(TEntity), otherFilters.Values.First().Parameters.Single().Name)
                : null;
            return AddOtherFilters<TEntity>(null, parameter, otherFilters);
        }

        public static Expression<Func<TEntity, bool>> AddOtherFilters<TEntity>(
            BinaryExpression initialExpression,
            ParameterExpression parameter,
            Dictionary<Type, Expression<Func<object, bool>>> otherFilters)
        {
            if (!otherFilters.Any(x => x.Key.IsAssignableFrom(typeof(TEntity))))
                //no other filters to add, so go with the single one
                return initialExpression == null
                    ? (Expression<Func<TEntity, bool>>) null
                    : Lambda<Func<TEntity, bool>>(initialExpression, parameter);

            Expression result = initialExpression;
            foreach (var otherFilterType in otherFilters.Keys)
            {
                if (otherFilterType.IsAssignableFrom(typeof(TEntity)))
                {
                    var specificFilter = otherFilters[otherFilterType];
                    if (specificFilter.Parameters.Single().Name != parameter.Name)
                        throw new InvalidOperationException(
                            $"The filter parameter for {otherFilterType.Name} must must be the same in all usages , i.e. {parameter.Name}.");

                    if (result == null)
                        result = Invoke(otherFilters[otherFilterType], parameter);
                    else
                        result = AndAlso(result, Invoke(otherFilters[otherFilterType], parameter));
                }
            }

            return Lambda<Func<TEntity, bool>>(result, parameter);
        }
    }
}