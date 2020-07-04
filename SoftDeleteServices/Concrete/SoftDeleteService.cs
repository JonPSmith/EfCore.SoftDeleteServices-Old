// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Concrete.Internal;
using StatusGeneric;

namespace SoftDeleteServices.Concrete
{
    public class SoftDeleteService : ISoftDeleteService
    {
        private readonly DbContext _context;
        private readonly bool _notFoundAllowed;

        /// <summary>
        /// Ctor for SoftDeleteService
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notFoundAllowed">Defaults to not found being an error. Set to true if not found isn't an error</param>
        public SoftDeleteService(DbContext context, bool notFoundAllowed = false)
        {
            _context = context;
            _notFoundAllowed = notFoundAllowed;
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then soft deletes it
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status with Result holding the soft deleted entity class - Result can be null if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<TEntity> SetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete
        {
            return _context.CheckExecuteSoftDelete<TEntity>(_notFoundAllowed, SetSoftDelete, keyValues);
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then resets the soft delete flag so it is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status with Result holding the soft deleted entity class - Result can be null if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<TEntity> ResetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete
        {
            return _context.CheckExecuteSoftDelete<TEntity>(_notFoundAllowed, ResetSoftDelete, keyValues);
        }


        /// <summary>
        /// This soft deletes the entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="softDeleteThisEntity">Mustn't be null</param>
        /// <returns>Status - returns an error if the entity is already soft deleted</returns>
        public IStatusGeneric SetSoftDelete<TEntity>(TEntity softDeleteThisEntity)
            where TEntity : class, ISoftDelete
        {
            if (softDeleteThisEntity == null) throw new ArgumentNullException(nameof(softDeleteThisEntity));

            var keys = _context.Entry(softDeleteThisEntity).Metadata.GetForeignKeys();
            if (!keys.All(x => x.DependentToPrincipal?.IsCollection == true || x.PrincipalToDependent?.IsCollection == true))
                //This it is a one-to-one entity - setting a one-to-one as soft deleted causes problems when you try to create a replacement
                throw new InvalidOperationException("You cannot soft delete a one-to-one relationship. " +
                                                    "It causes problems if you try to create a new version.");

            var status = new StatusGenericHandler();
            if (softDeleteThisEntity.SoftDeleted)
                return status.AddError("This entry is already soft deleted.");

            softDeleteThisEntity.SoftDeleted = true;
            _context.SaveChanges();
            return status;
        }

        /// <summary>
        /// This resets soft delete flag so that entity is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="resetSoftDeleteThisEntity">Mustn't be null</param>
        /// <returns>Status - returns an error if the entity's soft deleted flag isn't set</returns>
        public IStatusGeneric ResetSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
            where TEntity : class, ISoftDelete
        {
            if (resetSoftDeleteThisEntity == null) throw new ArgumentNullException(nameof(resetSoftDeleteThisEntity));

            var status = new StatusGenericHandler();
            if (!resetSoftDeleteThisEntity.SoftDeleted)
                return status.AddError("This entry isn't soft deleted.");

            resetSoftDeleteThisEntity.SoftDeleted = false;
            _context.SaveChanges();
            return status;
        }

        /// <summary>
        /// This returns the soft deleted entities of type TEntity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ISoftDelete
        {
            return _context.Set<TEntity>().IgnoreQueryFilters().Where(x => x.SoftDeleted);
        }


    }
}