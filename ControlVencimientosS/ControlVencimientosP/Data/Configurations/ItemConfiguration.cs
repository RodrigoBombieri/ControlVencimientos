using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> constructor)
    {
        constructor.ToTable("Items");
        constructor.Property(i => i.Nombre).HasMaxLength(200).IsRequired();
        constructor.Property(i => i.Codigo).HasMaxLength(60);
        constructor.Property(i => i.Ubicacion).HasMaxLength(160);
        constructor.Property(i => i.Proveedor).HasMaxLength(160);
        constructor.Property(i => i.Notas).HasMaxLength(2000);

        constructor.HasIndex(i => new { i.EmpresaId, i.CategoriaId });
        constructor.HasIndex(i => new { i.EmpresaId, i.Nombre });

        constructor.HasOne(i => i.Empresa)
                   .WithMany(e => e.Items)
                   .HasForeignKey(i => i.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(i => i.Categoria)
                   .WithMany(c => c.Items)
                   .HasForeignKey(i => i.CategoriaId)
                   .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(i => i.ResponsableUsuario)
                   .WithMany()
                   .HasForeignKey(i => i.ResponsableUsuarioId)
                   .OnDelete(DeleteBehavior.SetNull);
    }
}
