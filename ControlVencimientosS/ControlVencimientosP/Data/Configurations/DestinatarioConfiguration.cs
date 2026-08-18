using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class DestinatarioConfiguration : IEntityTypeConfiguration<Destinatario>
{
    public void Configure(EntityTypeBuilder<Destinatario> constructor)
    {
        constructor.ToTable("Destinatarios");
        constructor.Property(d => d.Nombre).HasMaxLength(160).IsRequired();
        constructor.Property(d => d.Email).HasMaxLength(256);
        constructor.Property(d => d.Telefono).HasMaxLength(40);
        constructor.Property(d => d.CategoriasFiltro).HasMaxLength(400);

        constructor.HasIndex(d => new { d.EmpresaId, d.Activo });

        constructor.HasOne(d => d.Empresa)
                   .WithMany()
                   .HasForeignKey(d => d.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}
