// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.Interfaces;
using StatusGeneric;

namespace SoftDeleteServices
{
    /// <summary>
    /// This interface contains the methods in the CascadeSoftDelService
    /// </summary>
    public interface ICascadeSoftDelService
    {
        /// <summary>
        /// This with cascade soft delete this entity and any dependent entities with the correct delete behaviour
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="softDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <param name="readEveryTime">defaults to reading all collections. See documentation on how this can be used to improve performance</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities that had been cascaded deleted, plus summary string in Message part</returns>
        IStatusGeneric<int> SetCascadeSoftDelete<TEntity>(TEntity softDeleteThisEntity, bool readEveryTime = true)
            where TEntity : class, ICascadeSoftDelete;

        /// <summary>
        /// This will result the cascade soft delete flag on this entity and any dependent entities with the correct delete behaviour and cascade level
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="resetSoftDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities that had been reset, plus summary string in Message part</returns>
        IStatusGeneric<int> ResetCascadeSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete;

        /// <summary>
        /// This looks for this entity and any dependent entities that are already been cascade soft deleted and are valid to be hard deleted. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="checkHardDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities which are eligible for hard delete, plus summary string in Message part</returns>
        IStatusGeneric<int> CheckCascadeSoftDelete<TEntity>(TEntity checkHardDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete;

        /// <summary>
        /// This hard deletes this entity and any dependent entities that are already been cascade soft deleted  
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="hardDeleteThisEntity">entity class with cascade soft delete interface. Mustn't be null</param>
        /// <returns>Returns a status. If no errors then Result contains the number of entities which were hard deleted, plus summary string in Message part</returns>
        IStatusGeneric<int> HardDeleteSoftDeletedEntries<TEntity>(TEntity hardDeleteThisEntity)
            where TEntity : class, ICascadeSoftDelete;

        /// <summary>
        /// This returns the cascade soft deleted entities of type TEntity that can be reset, i.e. SoftDeleteLevel == 1
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ICascadeSoftDelete;
    }
}