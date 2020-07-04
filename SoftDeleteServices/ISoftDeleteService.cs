// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.Interfaces;
using StatusGeneric;

namespace SoftDeleteServices
{
    public interface ISoftDeleteService
    {
        /// <summary>
        /// This finds the entity using its primary key(s) and then soft deletes it
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if error of Not Found and notFoundAllowed is true</returns>
        IStatusGeneric<int> SetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This finds the entity using its primary key(s) and then resets the soft delete flag so it is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keyValues">primary key values</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if error of Not Found and notFoundAllowed is true</returns>
        IStatusGeneric<int> ResetSoftDeleteViaKeys<TEntity>(params object[] keyValues)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This soft deletes the entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="softDeleteThisEntity">Mustn't be null</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if errors</returns>
        IStatusGeneric<int> SetSoftDelete<TEntity>(TEntity softDeleteThisEntity)
            where TEntity : class, ISoftDelete;

        /// <summary>
        /// This resets soft delete flag is cleared so that entity is now visible
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="resetSoftDeleteThisEntity">Mustn't be null</param>
        /// <returns>Returns status. If not errors then Result return 1 to say it worked. Zero if errors</returns>
        IStatusGeneric<int> ResetSoftDelete<TEntity>(TEntity resetSoftDeleteThisEntity)
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