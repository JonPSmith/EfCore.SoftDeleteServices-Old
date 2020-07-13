// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.Interfaces;

namespace DataLayer.CascadeEfClasses
{
    public class Quote : ICascadeSoftDelete
    {
        public int Id { get; set; }

        public int CompanySoftCascade { get; set; }

        public Company BelongsTo { get; set; }

        public byte SoftDeleteLevel { get; set; }
    }
}