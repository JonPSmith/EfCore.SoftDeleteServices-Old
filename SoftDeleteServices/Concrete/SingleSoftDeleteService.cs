// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Concrete.Internal;
using SoftDeleteServices.Configuration;
using StatusGeneric;

namespace SoftDeleteServices.Concrete
{
    public class SingleSoftDeleteService<TYourSoftDeleteInterface>
        where TYourSoftDeleteInterface : class
    {
        private readonly DbContext _context;
        private readonly SoftDeleteConfiguration<TYourSoftDeleteInterface, bool> _config;

        /// <summary>
        /// Ctor for SoftDeleteService
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public SingleSoftDeleteService(DbContext context, SoftDeleteConfiguration<TYourSoftDeleteInterface, bool> config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then soft deletes it
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if error of Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> SetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, TYourSoftDeleteInterface
        {
            return _context.CheckExecuteSoftDelete<TEntity>(_config.NotFoundIsNotAnError, SetSoftDelete, keyValues);
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then resets the soft delete flag so it is now visible
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if error of Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> ResetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, TYourSoftDeleteInterface
        {
            return _context.CheckExecuteSoftDelete<TEntity>(_config.NotFoundIsNotAnError, ResetSoftDelete, keyValues);
        }

        /// <summary>
        /// This soft deletes the entity
        /// </summary>
        /// <param name="softDeleteThisEntity">Mustn't be null</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if error</returns>
        public IStatusGeneric<int> SetSoftDelete(TYourSoftDeleteInterface softDeleteThisEntity)
        {
            if (softDeleteThisEntity == null) throw new ArgumentNullException(nameof(softDeleteThisEntity));

            var keys = _context.Entry(softDeleteThisEntity).Metadata.GetForeignKeys();
            if (!keys.All(x => x.DependentToPrincipal?.IsCollection == true || x.PrincipalToDependent?.IsCollection == true))
                //This it is a one-to-one entity - setting a one-to-one as soft deleted causes problems when you try to create a replacement
                throw new InvalidOperationException("You cannot soft delete a one-to-one relationship. " +
                                                    "It causes problems if you try to create a new version.");

            var status = new StatusGenericHandler<int>();
            if (_config.GetSoftDeleteValue.Compile().Invoke(softDeleteThisEntity))
                return status.AddError($"This entry is already {_config.SoftDeletedTextPastTense}.");

            _config.SetSoftDeleteValue(softDeleteThisEntity, true);
            _context.SaveChanges();

            status.Message = $"Successfully {_config.SoftDeletedTextPastTense} this entry";
            status.SetResult(1);        //one changed
            return status;
        }

        /// <summary>
        /// This resets soft delete flag is cleared so that entity is now visible
        /// </summary>
        /// <param name="resetSoftDeleteThisEntity">Mustn't be null</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if errors</returns>
        public IStatusGeneric<int> ResetSoftDelete(TYourSoftDeleteInterface resetSoftDeleteThisEntity)
        {
            if (resetSoftDeleteThisEntity == null) throw new ArgumentNullException(nameof(resetSoftDeleteThisEntity));

            var status = new StatusGenericHandler<int>();
            if (!_config.GetSoftDeleteValue.Compile().Invoke(resetSoftDeleteThisEntity))
                return status.AddError($"This entry isn't {_config.SoftDeletedTextPastTense}.");

            _config.SetSoftDeleteValue(resetSoftDeleteThisEntity, false);
            _context.SaveChanges();

            status.Message = $"Successfully {_config.ResetSoftDeleteText} on this entry";
            status.SetResult(1);        //one changed
            return status;
        }

        /// <summary>
        /// This returns the soft deleted entities of type TEntity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ISingleSoftDelete
        {
            return _context.Set<TEntity>().IgnoreQueryFilters().Where(x => x.SoftDeleted);
        }


    }
}