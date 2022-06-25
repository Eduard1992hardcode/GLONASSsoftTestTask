
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GLONASSsoftTestTask.Infrastructure.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<StatisticTaskEntity> StatisticTasks { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<StatisticTaskResultEntity> StatisticResults { get; set; }

    }
}
