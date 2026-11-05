using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Events;

public class CategoryCreatedEventHandler : INotificationHandler<CategoryCreatedEvent>
{
    public Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Gerçek projede burada Mail atılır veya Redis temizlenir. Biz şimdilik log basıyoruz.
        Console.WriteLine($"[SİSTEM BİLDİRİMİ]: '{notification.Name}' adında yeni kategori eklendi. Önbellek temizleniyor...");
        
        return Task.CompletedTask;
    }
}