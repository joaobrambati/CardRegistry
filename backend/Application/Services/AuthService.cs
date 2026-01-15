using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHashService _hashService;
    private readonly JwtTokenService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository usuarioRepository, IHashService hashService, JwtTokenService jwtService, ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _hashService = hashService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Response<string>> Register(CadastroDto dto)
    {
        _logger.LogInformation("Tentativa de registrar usuário com email {Email}", dto.Email);

        var existe = await _usuarioRepository.GetByEmail(dto.Email);
        if (existe != null)
        {
            _logger.LogWarning("Registro falhou: usuário {Email} já existe", dto.Email);
            return new Response<string> { Status = false, Mensagem = "Usuário já existe" };
        }

        var usuario = new UsuarioAuth
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = _hashService.GerarHash(dto.Senha),
            CriadoEm = DateTime.Now
        };

        await _usuarioRepository.Create(usuario);
        await _usuarioRepository.SaveChangesAsync();

        _logger.LogInformation("Usuário {Email} registrado com sucesso", dto.Email);

        return new Response<string> { Status = true, Mensagem = "Usuário cadastrado com sucesso" };
    }

    public async Task<Response<AuthResponseDto>> Login(LoginDto dto)
    {
        _logger.LogInformation("Tentativa de login do usuário {Email}", dto.Email);

        var usuario = await _usuarioRepository.GetByEmail(dto.Email);
        if (usuario == null)
        {
            _logger.LogWarning("Login falhou: usuário {Email} não encontrado", dto.Email);
            return new Response<AuthResponseDto> { Status = false, Mensagem = "Credenciais inválidas" };
        }

        var senhaHash = _hashService.GerarHash(dto.Senha);
        if (usuario.SenhaHash != senhaHash)
        {
            _logger.LogWarning("Login falhou: senha inválida para {Email}", dto.Email);
            return new Response<AuthResponseDto> { Status = false, Mensagem = "Credenciais inválidas" };
        }

        var token = _jwtService.GenerateToken(usuario);
        _logger.LogInformation("Login bem-sucedido para {Email}", dto.Email);

        return new Response<AuthResponseDto>
        {
            Data = new AuthResponseDto
            {
                Token = token,
                ExpiraEm = DateTime.UtcNow.AddMinutes(60)
            },
            Status = true,
            Mensagem = "Login realizado com sucesso"
        };
    }


}
