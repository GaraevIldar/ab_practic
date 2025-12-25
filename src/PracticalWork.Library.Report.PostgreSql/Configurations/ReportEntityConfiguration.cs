using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Report.PostgreSql.Entity;

namespace PracticalWork.Library.Report.PostgreSql.Configurations;

public class ReportEntityConfiguration: EntityConfigurationBase<ReportEntity>
{
    public override void Configure(EntityTypeBuilder<ReportEntity> builder)
    {
        base.Configure(builder);
        
        // Конфигурация строковых свойств с ограничением длины
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);
    
        builder.Property(e => e.FilePath)
            .IsRequired() 
            .HasMaxLength(500);
    
        // Конвертация enum в строку для хранения в БД
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>(
                reportStatus => reportStatus.ToString(),
                dbString => Enum.Parse<ReportStatus>(dbString));
    }
}