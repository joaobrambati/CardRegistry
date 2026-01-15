using Application.DTOs.Auth;
using Application.Services;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security;
using Moq;

namespace CardRegistryApi.Tests;

public class AuthServiceTests
    {
        // Fake do repositório
        private class FakeUsuarioRepository : IUsuarioRepository
        {
            private readonly List<UsuarioAuth> _store = new();
            public bool SaveChangesCalled { get; private set; } = false;
            public UsuarioAuth LastCreated { get; private set; }

            public Task<UsuarioAuth?> GetByEmail(string email)
            {
                var u = _store.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult<UsuarioAuth?>(u);
            }

            public Task Create(UsuarioAuth usuario)
            {
                _store.Add(usuario);
                LastCreated = usuario;
                return Task.CompletedTask;
            }

            public Task SaveChangesAsync()
            {
                SaveChangesCalled = true;
                return Task.CompletedTask;
            }

            public void Seed(UsuarioAuth usuario) => _store.Add(usuario);
        }

        // Fake do HashService
        private class FakeHashService : IHashService
        {
            public string GerarHash(string valor) => "HASH:" + valor;
        }

        // ----------------- TESTES -----------------

        [Fact]
        public async Task Register_WhenEmailAlreadyExists_ReturnsError()
        {
            var repo = new FakeUsuarioRepository();
            var hash = new FakeHashService();
            var jwt = new JwtTokenService(null!); // Não precisa gerar token nesse teste
            repo.Seed(new UsuarioAuth { Id = Guid.NewGuid(), Nome = "João", Email = "ja@existe.com", SenhaHash = "HASH:abc", CriadoEm = DateTime.Now });

            var service = new AuthService(repo, hash, jwt);
            var dto = new CadastroDto { Nome = "Novo", Email = "ja@existe.com", Senha = "abcd" };

            var result = await service.Register(dto);

            Assert.False(result.Status);
            Assert.Equal("Usuário já existe", result.Mensagem);
        }

        [Fact]
        public async Task Register_WhenNewUser_CreatesUserAndReturnsSuccess()
        {
            var repo = new FakeUsuarioRepository();
            var hash = new FakeHashService();
            var jwt = new JwtTokenService(null!); // Token não usado

            var service = new AuthService(repo, hash, jwt);
            var dto = new CadastroDto { Nome = "Novo", Email = "novo@teste.com", Senha = "1234" };

            var result = await service.Register(dto);

            Assert.True(result.Status);
            Assert.Equal("Usuário cadastrado com sucesso", result.Mensagem);
            Assert.NotNull(repo.LastCreated);
            Assert.Equal("HASH:1234", repo.LastCreated.SenhaHash);
            Assert.True(repo.SaveChangesCalled);
        }

        [Fact]
        public async Task Login_WhenUserNotFound_ReturnsInvalidCredentials()
        {
            var repo = new FakeUsuarioRepository();
            var hash = new FakeHashService();
            var jwt = new JwtTokenService(null!);

            var service = new AuthService(repo, hash, jwt);
            var dto = new LoginDto { Email = "nao@existe.com", Senha = "1234" };

            var result = await service.Login(dto);

            Assert.False(result.Status);
            Assert.Equal("Credenciais inválidas", result.Mensagem);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Login_WhenWrongPassword_ReturnsInvalidCredentials()
        {
            var repo = new FakeUsuarioRepository();
            var hash = new FakeHashService();
            var jwt = new JwtTokenService(null!);

            repo.Seed(new UsuarioAuth { Id = Guid.NewGuid(), Nome = "Usu", Email = "user@teste.com", SenhaHash = "HASH:senha-correta", CriadoEm = DateTime.Now });

            var service = new AuthService(repo, hash, jwt);
            var dto = new LoginDto { Email = "user@teste.com", Senha = "senha-errada" };

            var result = await service.Login(dto);

            Assert.False(result.Status);
            Assert.Equal("Credenciais inválidas", result.Mensagem);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Login_WhenCredentialsValid_ReturnsSuccess()
        {
            var repo = new FakeUsuarioRepository();
            var hash = new FakeHashService();
            var jwt = new JwtTokenService(null!); // Aqui só precisamos que o método não quebre

            var senha = "senha-correta";
            repo.Seed(new UsuarioAuth { Id = Guid.NewGuid(), Nome = "Usu", Email = "user@teste.com", SenhaHash = "HASH:" + senha, CriadoEm = DateTime.Now });

            var service = new AuthService(repo, hash, jwt);
            var dto = new LoginDto { Email = "user@teste.com", Senha = senha };

            var result = await service.Login(dto);

            Assert.True(result.Status);
            Assert.Equal("Login realizado com sucesso", result.Mensagem);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Token); // Só verificamos se gerou algum token
        }
    }
