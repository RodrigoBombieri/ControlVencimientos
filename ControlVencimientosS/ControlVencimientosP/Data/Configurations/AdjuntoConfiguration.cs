using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class AdjuntoConfiguration : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> constructor)
    {
        constructor.ToTable("Adjuntos");
        constructor.Property(a => a.NombreArchivo).HasMaxLength(260).IsRequired();
        constructor.Property(a => a.RutaBlob).HasMaxLength(500).IsRequired();
        constructor.Property(a => a.ContentType).HasMaxLength(120);

        constructor.HasIndex(a => a.VencimientoId);

        constructor.HasOne(a => a.Vencimiento)
                   .WithMany(v => v.Adjuntos)
                   .HasForeignKey(a => a.VencimientoId)
                   .OnDelete(DeleteBehavior.Cascade);
    }
}
