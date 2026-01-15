using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class UsuarioAuth
{
    [Key]
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public DateTime CriadoEm { get; set; }
}
