using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Usuario
{
    [Key]
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Senha { get; set; }
    public DateTime CriadoEm { get; set; }
}
