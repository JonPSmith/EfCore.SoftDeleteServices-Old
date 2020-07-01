// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.Interfaces;

namespace DataLayer.EfClasses
{
    public class BookSoftDel : ISoftDelete
    {
        public int BookSoftDelId { get; set; }
        public string Title { get; set; }
        public bool SoftDeleted { get; set; }

        public ICollection<ReviewSoftDel> Reviews { get; set; }
    }
}