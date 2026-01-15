using Domain.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface ICartaoRepository
{
    Task<Cartao?> GetByCardNumberHash(string cardNumber);
    Task Create(Cartao cartao);
    Task SaveChangesAsync();
}
