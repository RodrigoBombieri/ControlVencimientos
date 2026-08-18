using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> constructor)
    {
        constructor.ToTable("Categorias");
        constructor.Property(c => c.Nombre).HasMaxLength(80).IsRequired();
        constructor.Property(c => c.Icono).HasMaxLength(60).IsRequired();
        constructor.Property(c => c.Color).HasMaxLength(9).IsRequired();

        // No dos categorias con el mismo nombre dentro de la misma empresa.
        constructor.HasIndex(c => new { c.EmpresaId, c.Nombre }).IsUnique();

        constructor.HasOne(c => c.Empresa)
                   .WithMany(e => e.Categorias)
                   .HasForeignKey(c => c.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}
