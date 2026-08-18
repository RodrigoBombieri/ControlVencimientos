using ControlVencimientosP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlVencimientosP.Data.Configurations;

public class ConfiguracionAvisosConfiguration : IEntityTypeConfiguration<ConfiguracionAvisos>
{
    public void Configure(EntityTypeBuilder<ConfiguracionAvisos> constructor)
    {
        constructor.ToTable("ConfiguracionesAvisos");

        // Una sola fila por empresa: la PK es la FK.
        constructor.HasKey(c => c.EmpresaId);

        constructor.Property(c => c.HitosDias).HasMaxLength(120).IsRequired();

        constructor.HasOne(c => c.Empresa)
                   .WithOne()
                   .HasForeignKey<ConfiguracionAvisos>(c => c.EmpresaId)
                   .OnDelete(DeleteBehavior.Cascade);
    }
}
