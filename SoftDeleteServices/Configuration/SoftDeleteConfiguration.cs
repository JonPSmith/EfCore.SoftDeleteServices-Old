// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SoftDeleteServices.Configuration
{
    public class SoftDeleteConfiguration<TInterface, TYourValue>
        where TInterface : class
        where TYourValue : struct
    {
        /// <summary>
        /// This should contain a LINQ query that returns the soft delete property value - MUST work in EF Core query
        /// e.g. 
        /// </summary>
        public Expression<Func<TInterface, TYourValue>> GetSoftDeleteValue { get; set; }
        public Action<TInterface, TYourValue> SetSoftDeleteValue { get; set; }

        public Dictionary<Type, Expression<Func<object, bool>>> OtherFilters { get; } = new Dictionary<Type, Expression<Func<object, bool>>>();

        /// <summary>
        /// If this property is set to true, then it won't produce an error 
        /// This is useful for Web APIs where not finding something require a different return
        /// </summary>
        public bool NotFoundIsNotAnError { get; set; }

        /// <summary>
        /// This text is used in various 
        /// </summary>
        public string TextSoftDeletedPastTense { get; set; } = "soft deleted";

        public string TextHardDeletedPastTense { get; set; } = "hard deleted";

        public string TextResetSoftDelete { get; set; } = "reset the soft delete";

        //------------------------------------------------
        //Cascade only properties
        public bool ReadEveryTime { get; set; } = true;
    }
}