using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Recommendo_api.Models
{
    public partial class RecommendationContext : DbContext
    {
        public RecommendationContext(DbContextOptions<RecommendationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Recommendation> Recommendations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Recommendation>().ToTable("Recommendations");
            base.OnModelCreating(modelBuilder);
        }*/
    }
}
