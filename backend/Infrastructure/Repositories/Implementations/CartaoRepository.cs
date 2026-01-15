using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

public class CartaoRepository : ICartaoRepository
{
    private readonly AppDbContext _context;

    public CartaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cartao?> GetByCardNumberHash(string cardHash)
    {
        return await _context.Cartoes
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.NumeroCartaoHash == cardHash);
    }        

    public async Task Create(Cartao cartao)
        => await _context.Cartoes.AddAsync(cartao);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

}
