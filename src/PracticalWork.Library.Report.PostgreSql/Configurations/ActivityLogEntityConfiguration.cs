using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalWork.Library.Report.PostgreSql.Entity;

namespace PracticalWork.Library.Report.PostgreSql.Configurations;

public class ActivityLogEntityConfiguration: EntityConfigurationBase<ActivityLogEntity>
{
    public override void Configure(EntityTypeBuilder<ActivityLogEntity> builder)
    {
        base.Configure(builder);
        
        // Конфигурация обязательных свойств
        builder.Property(e => e.EventType)
            .IsRequired();
    
        // Конфигурация JSON свойства
        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");
    }
}