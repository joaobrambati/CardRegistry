using Application.DTOs.Cartao;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CartaoService : ICartaoService
{
    private readonly ICartaoRepository _repository;
    private readonly IHashService _hashService;
    private readonly ILogger<CartaoService> _logger;

    public CartaoService(ICartaoRepository repository, IHashService hashService, ILogger<CartaoService> logger)
    {
        _repository = repository;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<Response<ConsultarCartaoDto>> GetByCardNumber(string cardNumber)
    {
        _logger.LogInformation("GetByCardNumber chamado com numero {CardNumber}", cardNumber);

        try
        {
            var hash = _hashService.GerarHash(cardNumber.Trim());

            var cartao = await _repository.GetByCardNumberHash(hash);

            if (cartao is null)
            {
                _logger.LogWarning("Cartão não encontrado para hash {Hash}", hash);
                return new Response<ConsultarCartaoDto> { Status = false, Mensagem = "Não existe um cartão cadastrado com esse número." };
            }

            var cartaoMapeado = new ConsultarCartaoDto
            {
                CartaoId = cartao.Id,
                UltimosQuatroDigitos = cartao.UltimosQuatroDigitos
            };

            _logger.LogInformation("Cartão encontrado: {CartaoId}", cartao.Id);

            return new Response<ConsultarCartaoDto> { Data = cartaoMapeado, Status = true, Mensagem = "Cartão exibido com sucesso." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cartão {CardNumber}", cardNumber);
            return new Response<ConsultarCartaoDto> { Status = false, Mensagem = ex.Message };
        }
    }

    public async Task<Response<CartaoDto>> Create(NumCartaoDto dto)
    {
        _logger.LogInformation("Create chamado para cartão {CardNumber}", dto.NumeroCartao);

        try
        {
            var numCartao = dto.NumeroCartao.Trim();
            var hash = _hashService.GerarHash(numCartao);

            var existe = await _repository.GetByCardNumberHash(hash);
            if (existe is not null)
            {
                _logger.LogWarning("Cartão já existe: {CardNumber}", numCartao);
                return new Response<CartaoDto> { Status = false, Mensagem = "Cartão já cadastrado." };
            }

            var cartao = new Cartao
            {
                Id = Guid.NewGuid(),
                NumeroCartaoHash = hash,
                UltimosQuatroDigitos = numCartao.Length >= 4 ? numCartao[^4..] : numCartao,
                OrigemCadastro = "Manual",
                CriadoEm = DateTime.Now,
            };

            await _repository.Create(cartao);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Cartão cadastrado com sucesso: {CartaoId}", cartao.Id);

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
            };

            return new Response<CartaoDto> { Data = cartaoDto, Status = true, Mensagem = "Cartão cadastrado com sucesso." };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar cartão {CardNumber}", dto.NumeroCartao);
            return new Response<CartaoDto> { Status = false, Mensagem = ex.Message };
        }
    }

    public async Task<Response<List<CartaoDto>>> CreateFromFile(IFormFile arquivo)
    {
        _logger.LogInformation("CreateFromFile chamado com arquivo {FileName}", arquivo?.FileName);

        var cartoesCriados = new List<CartaoDto>();

        try
        {
            if (arquivo is null || arquivo.Length == 0)
            {
                _logger.LogWarning("Arquivo inválido ou vazio");
                return new Response<List<CartaoDto>> { Status = false, Mensagem = "Arquivo inválido." };
            }

            using var stream = arquivo.OpenReadStream();
            using var reader = new StreamReader(stream);

            var linhas = new List<string>();

            while (!reader.EndOfStream)
            {
                var linha = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(linha))
                    linhas.Add(linha);
            }

            var count = 0;
            var header = linhas[0];
            var codigoLote = header.Substring(37, 8).Trim();
            var dataLoteString = linhas[0].Substring(29, 8).Trim();
            DateTime.TryParseExact(dataLoteString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataLote);

            for (int i = 1; i < linhas.Count - 1; i++)
            {
                var linha = linhas[i].Trim();

                var partes = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length < 2)
                    continue;

                var identificadorLinha = partes[0][0].ToString();
                var sequenciaLote = partes[0].Substring(1);
                var numeroCartao = partes[1];

                var hash = _hashService.GerarHash(numeroCartao);

                var existe = await _repository.GetByCardNumberHash(hash);
                if (existe is not null)
                    continue; // cartão ja existe, ignora

                count++;

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
                };

                await _repository.Create(cartao);
                await _repository.SaveChangesAsync();

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
                });
            }

            if (count == 0)
            {
                _logger.LogWarning("Nenhum cartão novo foi cadastrado a partir do arquivo {FileName}", arquivo.FileName);
                return new Response<List<CartaoDto>> { Status = false, Mensagem = "Nenhum cartão foi cadastrado. Todos os cartões do arquivo já existem na base." };
            }

            _logger.LogInformation("{Count} cartões cadastrados a partir do arquivo {FileName}", count, arquivo.FileName);
            return new Response<List<CartaoDto>> { Data = cartoesCriados, Status = true, Mensagem = $"{count} Cartões cadastrados a partir do arquivo com sucesso." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo {FileName}", arquivo?.FileName);
            return new Response<List<CartaoDto>> { Status = false, Mensagem = ex.Message };
        }
    }

}
