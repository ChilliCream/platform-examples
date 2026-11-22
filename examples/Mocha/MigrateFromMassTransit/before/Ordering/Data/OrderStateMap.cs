using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Ordering;

/// <summary>
/// Maps the order saga state to the database.
/// </summary>
public sealed class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
    }
}
