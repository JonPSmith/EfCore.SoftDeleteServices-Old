// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfClasses;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SoftDelDbContext : DbContext
    {
        /// <summary>
        /// This holds the current userId, or GUID.Empty if not given
        /// </summary>
        public Guid UserId { get; private set; }

        private readonly QueryFilterAutoConfig _queryFilterAuto;

        public SoftDelDbContext(DbContextOptions<SoftDelDbContext> options, Guid userId = default)
            : base(options)
        {
            UserId = userId;
            _queryFilterAuto = new QueryFilterAutoConfig(userId);
        }

        public DbSet<EmployeeSoftCascade> Employees { get; set; }
        public DbSet<EmployeeContract> Contracts { get; set; }

        public DbSet<BookSoftDel> Books { get; set; }

        public DbSet<OrderSingleSoftDelUserId> Orders { get; set; }

        public DbSet<CompanySoftCascade> Companies { get; set; }
        public DbSet<QuoteSoftCascade> Quotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeSoftCascade>()
                .HasMany(x => x.WorksFromMe)
                .WithOne(x => x.Manager)
                .HasForeignKey(x => x.ManagerEmployeeSoftDelId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<EmployeeSoftCascade>()
                .HasOne(x => x.Contract)
                .WithOne()
                .HasForeignKey<EmployeeContract>(x => x.EmployeeSoftCascadeId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<OrderSingleSoftDelUserId>().HasQueryFilter(x => !x.SoftDeleted && x.UserId == UserId);


            //This automatically configures the two types of soft deletes
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISingleSoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    if (typeof(IUserId).IsAssignableFrom(entityType.ClrType))
                        continue;
                        //_queryFilterAuto.SetQueryFilter(entityType, MyQueryFilterTypes.SingleSoftDeleteAndUserId);
                    else
                        entityType.AddSoftDeleteQueryFilter();
                }
                if (typeof(ICascadeSoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.AddCascadeSoftDeleteQueryFilter();
                }
            }
        }
    }
}
