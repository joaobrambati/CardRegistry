namespace Application.DTOs.Cartao;

public class ConsultarCartaoDto
{
    public Guid CartaoId { get; set; }
    public string? UltimosQuatroDigitos { get; set; }
}
