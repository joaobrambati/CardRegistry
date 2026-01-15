using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<UsuarioAuth?> GetByEmail(string email);
    Task Create(UsuarioAuth usuario);
    Task SaveChangesAsync();
}
