using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DotNetCoreLab.Application.Features.Products.Queries.GetProductById;
using DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;
using DotNetCoreLab.Application.Features.Products.Commands.UpdateProduct;
using DotNetCoreLab.Application.Features.Products.Commands.DeleteProduct;
using DotNetCoreLab.Application.Features.Products.Commands.CreateProduct;
using DotNetCoreLab.Application.Features.Products.Queries.SearchProducts;
using DotNetCoreLab.Application.Common.Interfaces;

namespace DotNetCoreLab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ProductsController(IMediator mediator, IFileStorageService fileStorageService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var productId = await mediator.Send(command);
        return Ok(new { Id = productId, Message = "Ürün başarıyla oluşturuldu." });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var kronometre = System.Diagnostics.Stopwatch.StartNew();
        
        var query = new GetAllProductsQuery();
        var result = await mediator.Send(query, cancellationToken);
        
        kronometre.Stop();

        return Ok(new 
        { 
            YanitSuresiMs = $"{kronometre.ElapsedMilliseconds} ms",
            Veriler = result 
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result is null)
            return NotFound(new { Message = "Ürün bulunamadı." });

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "URL'deki ID ile gönderilen ürün ID'si uyuşmuyor." });

        var isUpdated = await mediator.Send(command, cancellationToken);

        if (!isUpdated)
            return NotFound(new { Message = "Güncellenecek ürün bulunamadı." });

        return NoContent(); 
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var isDeleted = await mediator.Send(command, cancellationToken);

        if (!isDeleted)
            return NotFound(new { Message = "Silinecek ürün bulunamadı." });

        return NoContent();
    }

    // Elasticsearch ile Akıllı ve Bulanık Ürün Arama (Fuzzy Search)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Message = "Arama kelimesi (q) boş olamaz." });

        var query = new SearchProductsQuery(q);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { Message = "Lütfen yüklenecek bir fotoğraf seçin." });

        using var stream = file.OpenReadStream();
        var imageUrl = await fileStorageService.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        return Ok(new
        {
            Mesaj = "Fotoğraf başarıyla AWS S3 bulutuna yüklendi!",
            ImageUrl = imageUrl
        });
    }
}