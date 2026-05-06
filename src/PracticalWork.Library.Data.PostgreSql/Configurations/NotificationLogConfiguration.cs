using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalWork.Library.Data.PostgreSql.Entities;

namespace PracticalWork.Library.Data.PostgreSql.Configurations;

internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLogEntity>
{
    public void Configure(EntityTypeBuilder<NotificationLogEntity> builder)
    {
        builder.ToTable("NotificationLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NotificationType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SentAt).IsRequired();
        builder.HasIndex(x => new { x.BorrowId, x.NotificationType, x.SentAt });
    }
}
