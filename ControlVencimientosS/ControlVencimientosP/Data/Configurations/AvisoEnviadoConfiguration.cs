using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class AvisoEnviadoConfiguration : IEntityTypeConfiguration<AvisoEnviado>
{
    public void Configure(EntityTypeBuilder<AvisoEnviado> constructor)
    {
        constructor.ToTable("AvisosEnviados");
        constructor.Property(a => a.ProveedorMessageId).HasMaxLength(200);
        constructor.Property(a => a.Error).HasMaxLength(1000);

        // EL indice importante del proyecto. Es lo unico que impide que un
        // reintento de Hangfire o un reinicio del servidor le mande el mismo
        // aviso dos veces al mismo destinatario. Si se cae este indice, el
        // motor de notificaciones deja de ser idempotente.
        constructor.HasIndex(a => new { a.VencimientoId, a.HitoDias, a.Canal, a.DestinatarioId })
                   .IsUnique()
                   .HasDatabaseName("UX_AvisosEnviados_Idempotencia");

        constructor.HasOne(a => a.Vencimiento)
                   .WithMany()
                   .HasForeignKey(a => a.VencimientoId)
                   .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(a => a.Destinatario)
                   .WithMany()
                   .HasForeignKey(a => a.DestinatarioId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}
