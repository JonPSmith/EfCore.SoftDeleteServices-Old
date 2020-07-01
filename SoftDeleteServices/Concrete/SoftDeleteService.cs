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
    public class SoftDeleteService : StatusGenericHandler
    {
        private readonly DbContext _context;

        public SoftDeleteService(DbContext context)
        {
            _context = context;
        }


        public bool SetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            var entity = _context.LoadEntityViaPrimaryKeys<TEntity>(true, keyValues);
            if (entity == null)
            {
                AddError("Could not find the entry you ask for.");
                return false;
            }
            if (!(entity is ISoftDelete castToSoftDelete))
                throw new InvalidOperationException($"The class {entity.GetType().Name} doesn't support simple soft delete.");
            
            if (castToSoftDelete.SoftDeleted)
            {
                AddError("This entry is already soft deleted.");
                return true;
            }

            SetSoftDelete(castToSoftDelete);
            return true;
        }

        public bool ResetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete
        {
            var entity = _context.LoadEntityViaPrimaryKeys<TEntity>(true, keyValues);
            if (entity == null)
            {
                AddError("Could not find the entry you ask for.");
                return false;
            }
            if (!(entity is ISoftDelete castToSoftDelete))
                throw new InvalidOperationException($"The class {entity.GetType().Name} doesn't support simple soft delete.");

            if (!castToSoftDelete.SoftDeleted)
            {
                AddError("This entry isn't soft deleted.");
                return true;
            }

            ResetSoftDelete(castToSoftDelete);
            return true;
        }


        public void SetSoftDelete<TEntity>(TEntity softDeleteThisEntity)
            where TEntity : class, ISoftDelete
        {
            var keys = _context.Entry(softDeleteThisEntity).Metadata.GetForeignKeys();
            if (!keys.All(x => x.DependentToPrincipal?.IsCollection == true || x.PrincipalToDependent?.IsCollection == true))
                //This it is a one-to-one entity - setting a one-to-one as soft deleted causes problems when you try to create a replacement
                throw new InvalidOperationException("You cannot soft delete a one-to-one relationship. " +
                                                    "It causes problems if you try to create a new version.");

            softDeleteThisEntity.SoftDeleted = true;
            _context.SaveChanges();
        }

        public void ResetSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
            where TEntity : class, ISoftDelete
        {
            resetSoftDeleteThisEntity.SoftDeleted = false;
            _context.SaveChanges();
        }

        public IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ISoftDelete
        {
            return _context.Set<TEntity>().IgnoreQueryFilters().Where(x => x.SoftDeleted);
        }
    }
}