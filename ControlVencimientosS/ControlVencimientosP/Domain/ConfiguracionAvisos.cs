namespace ControlVencimientosP.Domain;

/// <summary>Una fila por empresa. La PK es el EmpresaId.</summary>
public class ConfiguracionAvisos : IEntidadDeEmpresa
{
    public int EmpresaId { get; set; }

    /// <summary>Dias de anticipacion, separados por coma. 0 = el dia que vence.</summary>
    public string HitosDias { get; set; } = "60,30,15,7,1,0";

    /// <summary>Hora local de la empresa a la que sale el job diario.</summary>
    public TimeOnly HoraEnvio { get; set; } = new(8, 0);

    /// <summary>Cada cuantos dias se recuerda algo que ya vencio. 0 = no recordar.</summary>
    public int ReenviarVencidosCadaDias { get; set; } = 7;

    /// <summary>
    /// Un solo mail por dia agrupando todo. Es el default a proposito:
    /// la fatiga de alertas es el riesgo numero uno del producto.
    /// </summary>
    public bool DigestActivo { get; set; } = true;

    /// <summary>Mail aparte para lo urgente (7 dias o menos, o ya vencido).</summary>
    public bool AlertaIndividualActiva { get; set; } = true;

    public bool WhatsappActivo { get; set; }

    public Empresa? Empresa { get; set; }

    /// <summary>Parsea <see cref="HitosDias"/>. Devuelve los hitos ordenados y sin repetir.</summary>
    public IReadOnlyList<int> Hitos()
    {
        if (string.IsNullOrWhiteSpace(HitosDias))
            return [];

        return HitosDias
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => int.TryParse(t, out var d) ? d : int.MinValue)
            .Where(d => d >= 0)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();
    }
}
