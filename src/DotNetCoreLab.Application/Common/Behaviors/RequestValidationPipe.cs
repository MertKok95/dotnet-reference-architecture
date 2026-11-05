using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace DotNetCoreLab.Application.Common.Behaviors;

public class RequestValidationPipe<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _rules;

    public RequestValidationPipe(IEnumerable<IValidator<TRequest>> rules)
    {
        _rules = rules;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_rules.Any())
        {
            return await next();
        }

        var errorList = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            var checkResult = await rule.ValidateAsync(request, cancellationToken);
            if (!checkResult.IsValid)
            {
                errorList.AddRange(checkResult.Errors);
            }
        }

        if (errorList.Count > 0)
        {
            throw new ValidationException(errorList);
        }

        // 4. Hata yoksa asıl Handler'a geç
        return await next();
    }
}