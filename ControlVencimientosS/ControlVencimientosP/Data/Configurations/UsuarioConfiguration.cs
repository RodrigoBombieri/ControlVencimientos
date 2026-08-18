using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> constructor)
    {
        constructor.Property(u => u.NombreCompleto).HasMaxLength(160).IsRequired();
        constructor.HasIndex(u => u.EmpresaId);

        // Restrict y no Cascade: borrar una empresa no deberia borrar usuarios
        // en silencio, y ademas evita caminos de cascada multiples en SQL Server.
        constructor.HasOne(u => u.Empresa)
                   .WithMany()
                   .HasForeignKey(u => u.EmpresaId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}
