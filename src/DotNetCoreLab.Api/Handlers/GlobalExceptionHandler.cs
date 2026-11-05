using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreLab.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // --- DOĞRULAMA (VALIDATION) HATASI KONTROLÜ ---
        // Eğer hata bizim RequestValidationPipe filtresi aracılığı ile fırlatıldıysa bu bir sistem çökmesi değildir.
        // Kullanıcıya 400 Bad Request ve hangi alanları yanlış girdiğini döneriz.
        if (exception is ValidationException dogrulamaHatasi)
        {
            _logger.LogWarning("Doğrulama kuralı ihlali yakalandı.");

            // Hatalar alan adına göre (Name, Price, Stock) gruplanır.
            var alanHatalari = dogrulamaHatasi.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            var dogrulamaProblemDetayi = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Doğrulama Hatası (Validation Error)",
                Detail = "Gönderdiğiniz verilerde kurallara uymayan alanlar bulunuyor."
            };

            // ProblemDetails içine hatalı alanların listesini ekleriz.
            dogrulamaProblemDetayi.Extensions["hatalar"] = alanHatalari;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(dogrulamaProblemDetayi, cancellationToken);

            return true;
        }

        string gercekHataMesaji = exception.Message;
        string patladigiSatir = exception.StackTrace ?? "Bilinmiyor";
        string icHata = exception.InnerException?.Message ?? "Yok";

        if (exception is Microsoft.EntityFrameworkCore.DbUpdateException dbException)
        {
            // Sadece veritabanı hatalarına özel loglama
            _logger.LogError("Veritabanı Kayıt Hatası! Detay: {IcHata}, Mesaj: {Mesaj}", icHata, gercekHataMesaji);
        }
        else
        {
            // Diğer genel sistem hataları
            _logger.LogError(exception, "Kritik Sistem Çökmesi! Hata: {Mesaj}", gercekHataMesaji);
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Sunucu Hatası",
            Detail = "İşleminiz gerçekleştirilirken beklenmeyen bir hata oluştu. Teknik ekip bilgilendirildi." 
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}