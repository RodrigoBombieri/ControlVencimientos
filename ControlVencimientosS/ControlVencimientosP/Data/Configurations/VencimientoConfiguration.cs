using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class VencimientoConfiguration : IEntityTypeConfiguration<Vencimiento>
{
    public void Configure(EntityTypeBuilder<Vencimiento> constructor)
    {
        constructor.ToTable("Vencimientos");
        constructor.Property(v => v.NumeroDocumento).HasMaxLength(80);
        constructor.Property(v => v.Moneda).HasMaxLength(3);
        constructor.Property(v => v.Monto).HasPrecision(18, 2);

        // El indice del dashboard: filtra por estado y ordena por fecha.
        constructor.HasIndex(v => new { v.EmpresaId, v.Estado, v.FechaVencimiento });

        // La invariante del dominio, puesta en la base y no solo en el codigo:
        // un item no puede tener dos vencimientos activos al mismo tiempo.
        // Indice unico filtrado de SQL Server; Estado 0 = Activo.
        constructor.HasIndex(v => v.ItemId)
                   .IsUnique()
                   .HasFilter("[Estado] = 0")
                   .HasDatabaseName("UX_Vencimientos_UnoActivoPorItem");

        constructor.HasOne(v => v.Item)
                   .WithMany(i => i.Vencimientos)
                   .HasForeignKey(v => v.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(v => v.Empresa)
                   .WithMany()
                   .HasForeignKey(v => v.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);

        // Autorreferencia: apunta a la renovacion que lo reemplazo.
        constructor.HasOne(v => v.RenovadoPor)
                   .WithMany()
                   .HasForeignKey(v => v.RenovadoPorVencimientoId)
                   .OnDelete(DeleteBehavior.NoAction);
    }
}
