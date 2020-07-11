// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SoftDeleteServices.Configuration
{
    public class SoftDeleteConfiguration<TYourSoftDeleteInterface, TYourValue>
        where TYourSoftDeleteInterface : class
        where TYourValue : struct
    {
        public Expression<Func<TYourSoftDeleteInterface, TYourValue>> GetSoftDeleteValue { get; set; }
        public Action<TYourSoftDeleteInterface, TYourValue> SetSoftDeleteValue { get; set; }

        public Dictionary<Type, Expression<Func<object, bool>>> OtherFilters { get; } = new Dictionary<Type, Expression<Func<object, bool>>>();

        /// <summary>
        /// If this property is set to true, then it won't produce an error 
        /// This is useful for Web APIs where not finding something require a different return
        /// </summary>
        public bool NotFoundIsNotAnError { get; set; }

        /// <summary>
        /// This text is used in various 
        /// </summary>
        public string SoftDeletedTextPastTense { get; set; } = "soft deleted";

        public string ResetSoftDeleteText { get; set; } = "reset the soft delete";
    }
}