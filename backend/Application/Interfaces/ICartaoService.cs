using Application.DTOs.Cartao;
using Application.Response;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface ICartaoService
{
    Task<Response<ConsultarCartaoDto>> GetByCardNumber(string cardNumber);
    Task<Response<CartaoDto>> Create(NumCartaoDto dto);
    Task<Response<List<CartaoDto>>> CreateFromFile(IFormFile arquivo);
}
