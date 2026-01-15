using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioAuth?> GetByEmail(string email)
        => await _context.UsuariosAuth.FirstOrDefaultAsync(u => u.Email == email);

    public async Task Create(UsuarioAuth usuario)
        => await _context.UsuariosAuth.AddAsync(usuario);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

}
