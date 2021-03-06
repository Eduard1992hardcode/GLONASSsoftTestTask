using GLONASSsoftTestTask.Infrastructure.Models.BaseEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GLONASSsoftTestTask.Infrastructure.Models.Entities
{
    public class StatisticTaskEntity : IEntity<Guid>
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public decimal Percent { get; set; }
        public StatisticTaskResultEntity Result { get; set; }
        
    }
    class StatisticTaskEntityConfig : IEntityTypeConfiguration<StatisticTaskEntity>
    {
        public void Configure(EntityTypeBuilder<StatisticTaskEntity> builder)
        {
            builder.ToTable("StatisticTask", "TestTask");
            builder.HasKey(x => x.Id);
            builder.Property<DateTime>(c => c.DateStart).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            builder.Property<DateTime>(c => c.DateEnd).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }
}
