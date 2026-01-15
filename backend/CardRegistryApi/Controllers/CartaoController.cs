using Application.DTOs.Cartao;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CardRegistryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartaoController : ControllerBase
{
    private readonly ICartaoService _service;

    public CartaoController(ICartaoService cartaoService)
    {
        _service = cartaoService;
    }

    /// <summary>
    /// Verifica se um cartão existe a partir do seu número
    /// </summary>
    /// <returns></returns>
    [HttpGet("obterPorNumero/{cardNumber}")]
    public async Task<IActionResult> GetByCardNumber(string cardNumber)
    {
        var response = await _service.GetByCardNumber(cardNumber);

        if (!response.Status)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Cadastra um cartão informado manualmente
    /// </summary>
    /// <returns></returns>
    [HttpPost("cadastraManual")]
    public async Task<IActionResult> Create([FromBody] NumCartaoDto dto)
    {
        //var usuarioId = ObterUsuarioIdDoToken();
        var response = await _service.Create(dto, Guid.NewGuid());

        if (!response.Status)
            return BadRequest(response);

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Cadastra cartões a partir de um arquivo TXT
    /// </summary>
    /// <returns></returns>
    [HttpPost("cadastraArquivo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateFromFile([FromForm] UploadCartaoArquivoDto dto)
    {
        if (dto.Arquivo is null || dto.Arquivo.Length == 0)
            return BadRequest("Arquivo não informado.");

        //var usuarioId = ObterUsuarioIdDoToken();

        var response = await _service.CreateFromFile(dto.Arquivo, Guid.NewGuid());

        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    private Guid ObterUsuarioIdDoToken()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");

        return Guid.Parse(userIdClaim!.Value);
    }

}
