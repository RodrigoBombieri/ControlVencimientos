using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> constructor)
    {
        constructor.ToTable("Empresas");
        constructor.Property(e => e.Nombre).HasMaxLength(160).IsRequired();
        constructor.Property(e => e.Cuit).HasMaxLength(20);
        constructor.Property(e => e.ZonaHoraria).HasMaxLength(80).IsRequired();
        constructor.Property(e => e.LogoUrl).HasMaxLength(500);
    }
}
