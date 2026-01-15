using Application.DTOs.Cartao;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Application.Services;

public class CartaoService : ICartaoService
{
    private readonly ICartaoRepository _repository;
    private readonly IHashService _hashService;

    public CartaoService(ICartaoRepository repository, IHashService hashService)
    {
        _repository = repository;
        _hashService = hashService;
    }

    public async Task<Response<ConsultarCartaoDto>> GetByCardNumber(string cardNumber)
    {
        try
        {
            var hash = _hashService.GerarHash(cardNumber.Trim());

            var cartao = await _repository.GetByCardNumberHash(hash);

            if (cartao is null)
                return new Response<ConsultarCartaoDto> { Status = false, Mensagem = "Não existe um cartão cadastrado com esse número." };

            var cartaoMapeado = new ConsultarCartaoDto
            {
                CartaoId = cartao.Id,
                UltimosQuatroDigitos = cartao.UltimosQuatroDigitos
            };

            return new Response<ConsultarCartaoDto> { Data = cartaoMapeado, Status = true, Mensagem = "Cartão exibido com sucesso." };
        }
        catch(Exception ex)
        {
            return new Response<ConsultarCartaoDto> { Status = false, Mensagem = ex.Message };
        }
    }

    public async Task<Response<CartaoDto>> Create(NumCartaoDto dto, Guid usuarioId)
    {
        try
        {
            var numCartao = dto.NumeroCartao.Trim();
            var hash = _hashService.GerarHash(numCartao);

            var existe = await _repository.GetByCardNumberHash(hash);
            if (existe is not null)
                return new Response<CartaoDto> { Status = false, Mensagem = "Cartão já cadastrado." };

            var cartao = new Cartao
            {
                Id = Guid.NewGuid(),
                NumeroCartaoHash = hash,
                UltimosQuatroDigitos = numCartao.Length >= 4 ? numCartao[^4..] : numCartao,
                OrigemCadastro = "Manual",
                CriadoEm = DateTime.Now,
                UsuarioId = usuarioId                
            };

            await _repository.Create(cartao);
            await _repository.SaveChangesAsync();

            var cartaoDto = new CartaoDto
            {
                Id = cartao.Id,
                NumeroCartaoHash = cartao.NumeroCartaoHash,
                UltimosQuatroDigitos = cartao.UltimosQuatroDigitos,
                IdentificadorLinha = cartao.IdentificadorLinha,
                SequenciaLote = cartao.SequenciaLote,
                CodigoLote = cartao.CodigoLote,
                DataLote = cartao.DataLote,
                OrigemCadastro = cartao.OrigemCadastro,
                CriadoEm = cartao.CriadoEm,
                UsuarioId = cartao.UsuarioId
            };

            return new Response<CartaoDto> { Data = cartaoDto, Status = true, Mensagem = "Cartão cadastrado com sucesso." };

        }
        catch(Exception ex)
        {
            return new Response<CartaoDto> { Status = false, Mensagem = ex.Message };
        }
    }

    public async Task<Response<List<CartaoDto>>> CreateFromFile(IFormFile arquivo, Guid usuarioId)
    {
        var cartoesCriados = new List<CartaoDto>();

        try
        {
            if (arquivo is null || arquivo.Length == 0)
                return new Response<List<CartaoDto>> { Status = false, Mensagem = "Arquivo inválido." };

            using var stream = arquivo.OpenReadStream();
            using var reader = new StreamReader(stream);

            var linhas = new List<string>();

            while (!reader.EndOfStream)
            {
                var linha = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(linha))
                    linhas.Add(linha);
            }

            // Primeira linha é header e última linha é o footer do lote
            for (int i = 1; i < linhas.Count - 1; i++)
            {
                var linha = linhas[i];

                var numeroCartao = linha.Substring(7, 19).Trim();
                var hash = _hashService.GerarHash(numeroCartao);

                var existe = await _repository.GetByCardNumberHash(hash);
                if (existe is not null)
                    continue; // cartão ja existe, ignora

                var identificadorLinha = linha.Substring(0, 1).Trim();
                var sequenciaLote = linha.Substring(1, 6).Trim();
                var codigoLote = linhas[0].Substring(36, 8).Trim();
                var dataLoteString = linhas[0].Substring(29, 8).Trim(); 
                DateTime.TryParseExact(dataLoteString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataLote);

                var cartao = new Cartao
                {
                    Id = Guid.NewGuid(),
                    NumeroCartaoHash = hash,
                    UltimosQuatroDigitos = numeroCartao.Length >= 4 ? numeroCartao[^4..] : numeroCartao,
                    IdentificadorLinha = identificadorLinha,
                    SequenciaLote = sequenciaLote,
                    CodigoLote = codigoLote,
                    DataLote = dataLote,
                    OrigemCadastro = "Arquivo",
                    CriadoEm = DateTime.Now,
                    UsuarioId = usuarioId
                };

                await _repository.Create(cartao);

                cartoesCriados.Add(new CartaoDto
                {
                    Id = cartao.Id,
                    NumeroCartaoHash = cartao.NumeroCartaoHash,
                    UltimosQuatroDigitos = cartao.UltimosQuatroDigitos,
                    IdentificadorLinha = cartao.IdentificadorLinha,
                    SequenciaLote = cartao.SequenciaLote,
                    CodigoLote = cartao.CodigoLote,
                    DataLote = cartao.DataLote,
                    OrigemCadastro = cartao.OrigemCadastro,
                    CriadoEm = cartao.CriadoEm,
                    UsuarioId = cartao.UsuarioId
                });
            }

            await _repository.SaveChangesAsync();

            return new Response<List<CartaoDto>> { Data = cartoesCriados, Status = true, Mensagem = "Cartões cadastrados a partir do arquivo com sucesso." };
        }
        catch (Exception ex)
        {
            return new Response<List<CartaoDto>> { Status = false, Mensagem = ex.Message };
        }
    }

}
