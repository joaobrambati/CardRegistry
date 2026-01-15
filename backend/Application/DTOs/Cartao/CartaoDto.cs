namespace Application.DTOs.Cartao;

public class CartaoDto
{
    public Guid Id { get; set; }
    public string NumeroCartaoHash { get; set; } = null!;

    public string? UltimosQuatroDigitos { get; set; }
    public string? IdentificadorLinha { get; set; }

    public string? SequenciaLote { get; set; }
    public string? CodigoLote { get; set; }

    public DateTime? DataLote { get; set; }
    public string OrigemCadastro { get; set; } = null!;

    public DateTime CriadoEm { get; set; }
}
