using Application.DTOs.Cartao;
using Application.Services;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace CardRegistryApi.Tests;

public class CartaoServiceTests
{
    // --- Fakes simples ---
    private class FakeCartaoRepository : ICartaoRepository
    {
        private readonly List<Cartao> _store = new();
        public Cartao LastCreated { get; private set; }
        public bool SaveChangesCalled { get; private set; } = false;

        public Task<Cartao?> GetByCardNumberHash(string hash)
        {
            var c = _store.FirstOrDefault(x => x.NumeroCartaoHash == hash);
            return Task.FromResult<Cartao?>(c);
        }

        public Task Create(Cartao cartao)
        {
            _store.Add(cartao);
            LastCreated = cartao;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }

        public void Seed(Cartao cartao) => _store.Add(cartao);
    }

    private class FakeHashService : IHashService
    {
        public string GerarHash(string valor) => "HASH:" + valor;
    }

    // ---------------- TESTES ----------------

    [Fact]
    public async Task GetByCardNumber_WhenCardExists_ReturnsCartao()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        repo.Seed(new Cartao
        {
            Id = Guid.NewGuid(),
            NumeroCartaoHash = "HASH:1234567890123456",
            UltimosQuatroDigitos = "3456",
            CriadoEm = DateTime.Now,
            OrigemCadastro = "teste"
        });

        var service = new CartaoService(repo, hashService);

        var result = await service.GetByCardNumber("1234567890123456");

        Assert.True(result.Status);
        Assert.Equal("Cartão exibido com sucesso.", result.Mensagem);
        Assert.Equal("3456", result.Data.UltimosQuatroDigitos);
    }

    [Fact]
    public async Task GetByCardNumber_WhenCardDoesNotExist_ReturnsError()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        var service = new CartaoService(repo, hashService);

        var result = await service.GetByCardNumber("0000000000000000");

        Assert.False(result.Status);
        Assert.Equal("Não existe um cartão cadastrado com esse número.", result.Mensagem);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Create_WhenCardDoesNotExist_CreatesCard()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        var service = new CartaoService(repo, hashService);
        var dto = new NumCartaoDto { NumeroCartao = "1234567890123456" };

        var result = await service.Create(dto);

        Assert.True(result.Status);
        Assert.Equal("Cartão cadastrado com sucesso.", result.Mensagem);
        Assert.NotNull(repo.LastCreated);
        Assert.Equal("HASH:1234567890123456", repo.LastCreated.NumeroCartaoHash);
        Assert.True(repo.SaveChangesCalled);
    }

    [Fact]
    public async Task Create_WhenCardAlreadyExists_ReturnsError()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        repo.Seed(new Cartao
        {
            Id = Guid.NewGuid(),
            NumeroCartaoHash = "HASH:1234567890123456",
            UltimosQuatroDigitos = "3456",
            OrigemCadastro = "teste"
        });

        var service = new CartaoService(repo, hashService);
        var dto = new NumCartaoDto { NumeroCartao = "1234567890123456" };

        var result = await service.Create(dto);

        Assert.False(result.Status);
        Assert.Equal("Cartão já cadastrado.", result.Mensagem);
    }

    [Fact]
    public async Task CreateFromFile_WhenValidFile_CreatesCards()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        // Simulando arquivo CSV/Texto com 2 cartões
        var conteudo = new StringBuilder();
        conteudo.AppendLine("HEADER12345620260115"); // linha de header (data e código)
        conteudo.AppendLine("A1 1234567890123456");
        conteudo.AppendLine("B2 6543210987654321");
        conteudo.AppendLine(""); // linha vazia final

        var bytes = Encoding.UTF8.GetBytes(conteudo.ToString());
        var stream = new MemoryStream(bytes);

        IFormFile file = new FormFile(stream, 0, bytes.Length, "arquivo", "teste.txt");

        var service = new CartaoService(repo, hashService);

        var result = await service.CreateFromFile(file);

        Assert.True(result.Status);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("2 Cartões cadastrados a partir do arquivo com sucesso.", result.Mensagem);
    }

    [Fact]
    public async Task CreateFromFile_WhenAllCardsExist_ReturnsError()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        // Já cadastrando os cartões
        repo.Seed(new Cartao { Id = Guid.NewGuid(), NumeroCartaoHash = "HASH:1234567890123456", UltimosQuatroDigitos = "3456", OrigemCadastro = "teste" });
        repo.Seed(new Cartao { Id = Guid.NewGuid(), NumeroCartaoHash = "HASH:6543210987654321", UltimosQuatroDigitos = "4321", OrigemCadastro = "teste" });

        var conteudo = new StringBuilder();
        conteudo.AppendLine("HEADER12345620260115");
        conteudo.AppendLine("A1 1234567890123456");
        conteudo.AppendLine("B2 6543210987654321");
        conteudo.AppendLine("");

        var bytes = Encoding.UTF8.GetBytes(conteudo.ToString());
        var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, bytes.Length, "arquivo", "teste.txt");

        var service = new CartaoService(repo, hashService);

        var result = await service.CreateFromFile(file);

        Assert.False(result.Status);
        Assert.Equal("Nenhum cartão foi cadastrado. Todos os cartões do arquivo já existem na base.", result.Mensagem);
    }

    [Fact]
    public async Task CreateFromFile_WhenFileIsInvalid_ReturnsError()
    {
        var repo = new FakeCartaoRepository();
        var hashService = new FakeHashService();

        IFormFile file = null; // arquivo nulo

        var service = new CartaoService(repo, hashService);

        var result = await service.CreateFromFile(file);

        Assert.False(result.Status);
        Assert.Equal("Arquivo inválido.", result.Mensagem);
    }
}
