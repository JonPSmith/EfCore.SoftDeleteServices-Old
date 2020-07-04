// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.Interfaces;
using StatusGeneric;

namespace SoftDeleteServices
{
    /// <summary>
    /// This interface contains the methods in the SoftDeleteService
    /// </summary>
    public interface ISoftDeleteService
    {
        /// <summary>
        /// This finds the entity using its primary key(s) and then soft deletes it
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status with Result holding the soft deleted entity class - Result can be null if Not Found and notFoundAllowed is true</returns>
        IStatusGeneric<TEntity> SetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This finds the entity using its primary key(s) and then resets the soft delete flag so it is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status with Result holding the soft deleted entity class - Result can be null if Not Found and notFoundAllowed is true</returns>
        IStatusGeneric<TEntity> ResetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This soft deletes the entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="softDeleteThisEntity">Mustn't be null</param>
        /// <returns>Status - returns an error if the entity is already soft deleted</returns>
        IStatusGeneric SetSoftDelete<TEntity>(TEntity softDeleteThisEntity)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This resets soft delete flag so that entity is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="resetSoftDeleteThisEntity">Mustn't be null</param>
        /// <returns>Status - returns an error if the entity's soft deleted flag isn't set</returns>
        IStatusGeneric ResetSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This returns the soft deleted entities of type TEntity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IQueryable<TEntity> GetSoftDeletedEntries<TEntity>()
            where TEntity : class, ISoftDelete;
    }
}