using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Cartao;

public class UploadCartaoArquivoDto
{
    public required IFormFile Arquivo { get; set; }
}
