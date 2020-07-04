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

    public class CascadeSoftDelService : ICascadeSoftDelService
    {
        public enum CascadeSoftDelWhatDoing { SoftDelete, ResetSoftDelete, CheckWhatWillDelete, HardDeleteSoftDeleted }

        private readonly DbContext _context;
        private readonly bool _notFoundAllowed;

        /// <summary>
        /// This provides a equivalent to a SQL cascade delete, but using a soft delete approach.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notFoundAllowed">Defaults to not found being an error. Set to true if not found isn't an error</param>
        public CascadeSoftDelService(DbContext context, bool notFoundAllowed = false)
        {
            _context = context;
            _notFoundAllowed = notFoundAllowed;
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then cascade soft deletes the entity any dependent entities with the correct delete behaviour
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result has the number of entities that have been soft deleted. Is -1 if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> SetCascadeSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ICascadeSoftDelete
        {
            return _context.CheckExecuteCascadeSoftDelete<TEntity>(_notFoundAllowed, x=>  SetCascadeSoftDelete(x), keyValues);
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and then resets the soft delete flag so it is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result has the number of entities that have been reset. Is -1 if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> ResetCascadeSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ICascadeSoftDelete
        {
            return _context.CheckExecuteCascadeSoftDelete<TEntity>(_notFoundAllowed, ResetCascadeSoftDelete, keyValues);
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and counts this entity and any dependent entities
        /// that are already been cascade soft deleted and are valid to be hard deleted.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Message contains a message to warn what will be deleted if the HardDelete... method is called.
        /// Result is -1 if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> CheckCascadeSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ICascadeSoftDelete
        {
            return _context.CheckExecuteCascadeSoftDelete<TEntity>(_notFoundAllowed, CheckCascadeSoftDelete, keyValues);
        }

        /// <summary>
        /// This finds the entity using its primary key(s) and hard deletes this entity and any dependent entities
        /// that are already been cascade soft deleted.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result has the number of entities that have been hard deleted. Is -1 if Not Found and notFoundAllowed is true</returns>
        public IStatusGeneric<int> HardDeleteSoftDeletedEntriesViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ICascadeSoftDelete
        {
            return _context.CheckExecuteCascadeSoftDelete<TEntity>(_notFoundAllowed, HardDeleteSoftDeletedEntries, keyValues);
        }

        /// <summary>
        /// This with cascade soft delete this entity and any dependent entities with the correct delete behaviour
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="softDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <param name="readEveryTime">defaults to reading all collections. See documentation on how this can be used to improve performance</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities that had been cascaded deleted, plus summary string in Message part</returns>
        public IStatusGeneric<int> SetCascadeSoftDelete<TEntity>(TEntity softDeleteThisEntity, bool readEveryTime = true)
            where TEntity : class, ICascadeSoftDelete
        {
            if (softDeleteThisEntity == null) throw new ArgumentNullException(nameof(softDeleteThisEntity));
            
            var status = new StatusGenericHandler<int>();

            //If is a one-to-one entity we return an error
            var keys = _context.Entry(softDeleteThisEntity).Metadata.GetForeignKeys();
            if (!keys.All(x => x.DependentToPrincipal?.IsCollection == true || x.PrincipalToDependent?.IsCollection == true))
                //This it is a one-to-one entity
                throw new InvalidOperationException("You cannot soft delete a one-to-one relationship. " +
                                                    "It causes problems if you try to create a new version.");

            if (softDeleteThisEntity.SoftDeleteLevel != 0)
                return status.AddError("This entry is already soft deleted");

            var walker = new CascadeWalker(_context, CascadeSoftDelWhatDoing.SoftDelete, readEveryTime);
            walker.WalkEntitiesSoftDelete(softDeleteThisEntity, 1);
            _context.SaveChanges();
            return ReturnSuccessFullResult(CascadeSoftDelWhatDoing.SoftDelete, walker.NumFound);
        }

        /// <summary>
        /// This will result the cascade soft delete flag on this entity and any dependent entities with the correct delete behaviour and cascade level
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="resetSoftDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities that had been reset, plus summary string in Message part</returns>

        public IStatusGeneric<int> ResetCascadeSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete
        {
            if (resetSoftDeleteThisEntity == null) throw new ArgumentNullException(nameof(resetSoftDeleteThisEntity));
            var status = new StatusGenericHandler<int>();
            if (resetSoftDeleteThisEntity.SoftDeleteLevel == 0)
                return status.AddError("This entry isn't soft deleted");

            if (resetSoftDeleteThisEntity.SoftDeleteLevel > 1)
                return status.AddError($"This entry was soft deleted {resetSoftDeleteThisEntity.SoftDeleteLevel - 1} " +
                    $"level{(resetSoftDeleteThisEntity.SoftDeleteLevel > 2  ? "s" : "")} above here");

            //For reset you need to read every time because some of the collection might be soft deleted already
            var walker = new CascadeWalker(_context, CascadeSoftDelWhatDoing.ResetSoftDelete, true);
            walker.WalkEntitiesSoftDelete(resetSoftDeleteThisEntity, 1);
            _context.SaveChanges();
            return ReturnSuccessFullResult(CascadeSoftDelWhatDoing.ResetSoftDelete, walker.NumFound);
        }

        /// <summary>
        /// This looks for this entity and any dependent entities that are already been cascade soft deleted and are valid to be hard deleted. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="checkHardDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities which are eligible for hard delete, plus summary string in Message part</returns>
        public IStatusGeneric<int> CheckCascadeSoftDelete<TEntity>(TEntity checkHardDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete
        {
            if (checkHardDeleteThisEntity == null) throw new ArgumentNullException(nameof(checkHardDeleteThisEntity));
            var status = new StatusGenericHandler<int>();
            if (checkHardDeleteThisEntity.SoftDeleteLevel == 0)
                return status.AddError("This entry isn't soft deleted");

            //For reset you need to read every time because some of the collection might be soft deleted already
            var walker = new CascadeWalker(_context, CascadeSoftDelWhatDoing.CheckWhatWillDelete, true);
            walker.WalkEntitiesSoftDelete(checkHardDeleteThisEntity, 1);
            return ReturnSuccessFullResult(CascadeSoftDelWhatDoing.CheckWhatWillDelete, walker.NumFound);
        }

        /// <summary>
        /// This hard deletes this entity and any dependent entities that are already been cascade soft deleted  
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="hardDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities which were hard deleted, plus summary string in Message part</returns>
        public IStatusGeneric<int> HardDeleteSoftDeletedEntries<TEntity>(TEntity hardDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete
        {
            if (hardDeleteThisEntity == null) throw new ArgumentNullException(nameof(hardDeleteThisEntity));
            var status = new StatusGenericHandler<int>();
            if (hardDeleteThisEntity.SoftDeleteLevel == 0)
                return status.AddError("This entry isn't soft deleted");

            //For reset you need to read every time because some of the collection might be soft deleted already
            var walker = new CascadeWalker(_context, CascadeSoftDelWhatDoing.HardDeleteSoftDeleted, true);
            walker.WalkEntitiesSoftDelete(hardDeleteThisEntity, 1);
            _context.SaveChanges();
            return ReturnSuccessFullResult(CascadeSoftDelWhatDoing.HardDeleteSoftDeleted, walker.NumFound);
        }


        /// <summary>
        /// This returns the cascade soft deleted entities of type TEntity that can be reset, i.e. SoftDeleteLevel == 1
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ICascadeSoftDelete
        {
            return _context.Set<TEntity>().IgnoreQueryFilters().Where(x => x.SoftDeleteLevel == 1);
        }

        //---------------------------------------------------------
        //private methods

        private IStatusGeneric<int> ReturnSuccessFullResult(CascadeSoftDelWhatDoing whatDoing, int numFound)
        {
            var status = new StatusGenericHandler<int>();
            status.SetResult(numFound);
            switch (whatDoing)
            {
                case CascadeSoftDelWhatDoing.SoftDelete:
                    status.Message = FormMessage("soft deleted", numFound);
                    break;
                case CascadeSoftDelWhatDoing.ResetSoftDelete:
                    status.Message = FormMessage("recovered", numFound);
                    break;
                case CascadeSoftDelWhatDoing.CheckWhatWillDelete:
                    status.Message = numFound == 0
                        ? "No entries will be hard deleted"
                        : $"Are you sure you want to hard delete this entity{DependentsSuffix(numFound)}";
                    break;
                case CascadeSoftDelWhatDoing.HardDeleteSoftDeleted:
                    status.Message = FormMessage("hard deleted", numFound);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return status;
        }

        private string FormMessage(string what, int numFound)
        {
            if (numFound == 0)
                return $"No entries have been {what}";
            var dependentsSuffix = numFound > 1
                ? $" and its {numFound - 1} dependents"
                : "";
            return $"You have {what} an entity{DependentsSuffix(numFound)}";
        }

        private string DependentsSuffix(int numFound)
        {
            return numFound > 1
                ? $" and its {numFound - 1} dependents"
                : "";
        }
    }
}