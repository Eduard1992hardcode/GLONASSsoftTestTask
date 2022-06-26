using GLONASSsoftTestTask.Infrastructure.Models.BaseEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GLONASSsoftTestTask.Infrastructure.Models.Entities
{
    public class StatisticTaskResultEntity : IEntity<Guid>
    {
        public Guid TaskId { get; set; }

        public int CountSignIn { get; set; }
        public StatisticTaskEntity Task { get; set; }
    }
    class StatisticTaskResultEntityConfig : IEntityTypeConfiguration<StatisticTaskResultEntity>
    {
        public void Configure(EntityTypeBuilder<StatisticTaskResultEntity> builder)
        {
            builder.ToTable("StatisticTaskResult", "TestTask");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Task)
                .WithOne()
                .HasForeignKey<StatisticTaskResultEntity>(x => x.TaskId);
        }
    }
}
