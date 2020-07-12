// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
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

        public Expression<Func<TEntity, bool>> FormFilterSingleSoftDelete<TEntity>()
            where TEntity : class, TInterface
        {
            var parameter = Parameter(typeof(TEntity), _config.GetSoftDeleteValue.Parameters.Single().Name);
            var left = Invoke(_config.GetSoftDeleteValue, parameter);
            var right = Constant(true);
            var result = Equal(left, right);

            if (!_config.OtherFilters.Any(x => x.Key.IsAssignableFrom(typeof(TEntity))))
                //no other filters to add, so go with the single one
                return Lambda<Func<TEntity, bool>>(result, parameter);

            foreach (var otherFilterType in _config.OtherFilters.Keys)
            {
                if (otherFilterType.IsAssignableFrom(typeof(TEntity)))
                {
                    var specificFilter = _config.OtherFilters[otherFilterType];
                    if (specificFilter.Parameters.Single().Name != _config.GetSoftDeleteValue.Parameters.Single().Name)
                        throw new InvalidOperationException(
                            $"The filter parameter for {otherFilterType.Name} must must match the " +
                            $"{nameof(_config.GetSoftDeleteValue)}, i.e. {_config.GetSoftDeleteValue.Parameters.Single().Name}.");
                    result = AndAlso(result,
                        Invoke(_config.OtherFilters[otherFilterType], parameter));
                }
            }

            return Lambda<Func<TEntity, bool>>(result, parameter);
        }

        public Expression<Func<TEntity, bool>> FormOtherFiltersOnly<TEntity>()
        {
            if (!_config.OtherFilters.Any(x => x.Key.IsAssignableFrom(typeof(TEntity))))
                //no other filters to add, so go with the single one
                return null;

            var parameter = Parameter(typeof(TEntity), _config.GetSoftDeleteValue.Parameters.Single().Name);
            Expression result = null;
            foreach (var otherFilterType in _config.OtherFilters.Keys)
            {
                if (otherFilterType.IsAssignableFrom(typeof(TEntity)))
                {
                    var specificFilter = _config.OtherFilters[otherFilterType];
                    if (specificFilter.Parameters.Single().Name != _config.GetSoftDeleteValue.Parameters.Single().Name)
                        throw new InvalidOperationException(
                            $"The filter parameter for {otherFilterType.Name} must must match the " +
                            $"{nameof(_config.GetSoftDeleteValue)}, i.e. {_config.GetSoftDeleteValue.Parameters.Single().Name}.");

                    if (result == null)
                        result = Invoke(_config.OtherFilters[otherFilterType], parameter);
                    else
                        result = AndAlso(result, Invoke(_config.OtherFilters[otherFilterType], parameter));
                }
            }

            return Lambda<Func<TEntity, bool>>(result, parameter);
        }
    }
}