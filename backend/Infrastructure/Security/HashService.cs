using Domain.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Infrastructure.Security;

public class HashService : IHashService
{
    private const int Iteracoes = 100_000;
    private readonly byte[] _salt;

    public HashService(IConfiguration configuration)
    {
        _salt = Convert.FromBase64String(configuration["Security:CardHashSalt"] ?? throw new InvalidOperationException("Salt não configurado"));
    }

    public string GerarHash(string valor)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(valor, _salt, Iteracoes, HashAlgorithmName.SHA256);

        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }
}

