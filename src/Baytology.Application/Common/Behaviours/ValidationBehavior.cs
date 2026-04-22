using MediatR;

namespace Baytology.Application.Common.Behaviours;

using FluentValidation;

using Baytology.Domain.Common.Results;
using Baytology.Domain.Common.Results.Abstractions;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
{
    private readonly IValidator<TRequest>? _validator = validator;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validator is null)
        {
            return await next(ct);
        }

        var validationResult = await _validator.ValidateAsync(request, ct);

        if (validationResult.IsValid)
        {
            return await next(ct);
        }

        var errors = validationResult.Errors
            .ConvertAll(error => ApplicationErrors.Validation.Pipeline(
                error.PropertyName,
                error.ErrorMessage));

        return CreateValidationResult(errors);
    }

    private static TResponse CreateValidationResult(List<Error> errors)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GenericTypeArguments[0];
            var defaultValue = valueType.IsValueType
                ? Activator.CreateInstance(valueType)
                : null;

            return (TResponse)Activator.CreateInstance(responseType, defaultValue, errors, false)!;
        }

        throw new InvalidOperationException($"ValidationBehavior cannot create an error result for response type '{responseType.Name}'.");
    }
}
