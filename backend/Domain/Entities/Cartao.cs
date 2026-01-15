using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Cartao
{
    [Key]
    public Guid Id { get; set; }
    public required string NumeroCartaoHash { get; set; }
    public string? UltimosQuatroDigitos { get; set; }
    public string? IdentificadorLinha { get; set; }
    public string? SequenciaLote { get; set; }
    public string? CodigoLote { get; set; }
    public DateTime? DataLote { get; set; }
    public required string OrigemCadastro { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
