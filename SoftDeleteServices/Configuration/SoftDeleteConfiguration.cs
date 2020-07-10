// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace SoftDeleteServices.Configuration
{
    public class SoftDeleteConfiguration<TYourSoftDeleteInterface>
        where TYourSoftDeleteInterface : class
    {
        public Func<object, SoftDeleteAccess<TYourSoftDeleteInterface>> CreateAccessor { get; set; }

        /// <summary>
        /// If this property is set to true, then it won't produce an error 
        /// This is useful for Web APIs where not finding something require a different return
        /// </summary>
        public bool NotFoundIsNotAnError { get; set; }

        /// <summary>
        /// This text is used in various 
        /// </summary>
        public string SoftDeletedTextPastTense { get; set; } = "soft deleted";

        public string ResetSoftDeleteTextPastTense { get; set; } = "soft delete was reset";
    }
}