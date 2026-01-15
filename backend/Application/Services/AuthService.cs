using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHashService _hashService;
    private readonly JwtTokenService _jwtService;

    public AuthService(IUsuarioRepository usuarioRepository, IHashService hashService, JwtTokenService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _hashService = hashService;
        _jwtService = jwtService;
    }

    public async Task<Response<string>> Register(CadastroDto dto)
    {
        var existe = await _usuarioRepository.GetByEmail(dto.Email);
        if (existe != null)
            return new Response<string> { Status = false, Mensagem = "Usuário já existe" };

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

        return new Response<string> { Status = true, Mensagem = "Usuário cadastrado com sucesso" };
    }

    public async Task<Response<AuthResponseDto>> Login(LoginDto dto)
    {
        var usuario = await _usuarioRepository.GetByEmail(dto.Email);
        if (usuario == null)
            return new Response<AuthResponseDto> { Status = false, Mensagem = "Credenciais inválidas" };

        var senhaHash = _hashService.GerarHash(dto.Senha);
        if (usuario.SenhaHash != senhaHash)
            return new Response<AuthResponseDto> { Status = false, Mensagem = "Credenciais inválidas" };

        var token = _jwtService.GenerateToken(usuario);

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
