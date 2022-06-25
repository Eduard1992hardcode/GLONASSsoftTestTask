using GLONASSsoftTestTask.Infrastructure.Models.BaseEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace GLONASSsoftTestTask.Infrastructure.Models.Entities
{
    public class UserEntity : IEntity<Guid>
    {
        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public List<StatisticTaskEntity> SatisticTasks { get; set; }
    }

    class UserConfig : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("User", "TestTask");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FirstName).HasMaxLength(15);
            builder.Property(x => x.SecondName).HasMaxLength(15);
            builder.HasMany(x => x.SatisticTasks)
                .WithOne()
                .HasForeignKey(x => x.UserId);
        }
    }
}
