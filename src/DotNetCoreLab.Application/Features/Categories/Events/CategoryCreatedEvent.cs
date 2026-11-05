using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Events;

// Bu bir komut değil, yaşanmış bir olaydır. Bu yüzden IRequest değil INotification kullanılır.
public record CategoryCreatedEvent(Guid Id, string Name) : INotification;