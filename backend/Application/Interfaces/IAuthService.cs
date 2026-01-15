using Application.DTOs.Auth;
using Application.Response;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<Response<string>> Register(CadastroDto dto);
    Task<Response<AuthResponseDto>> Login(LoginDto dto);
}
