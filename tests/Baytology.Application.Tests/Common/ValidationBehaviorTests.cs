using Baytology.Application.Common.Behaviours;
using Baytology.Domain.Common.Results;

using FluentValidation;

using MediatR;

namespace Baytology.Application.Tests.Common;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Invalid_request_returns_typed_error_result_without_dynamic_casts()
    {
        var validator = new TestCommandValidator();
        var behavior = new ValidationBehavior<TestCommand, Result<Success>>(validator);

        var result = await behavior.Handle(
            new TestCommand(string.Empty),
            _ => Task.FromResult<Result<Success>>(Result.Success),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Contains(result.Errors, error => error.Code == nameof(TestCommand.Name));
    }

    private sealed record TestCommand(string Name) : IRequest<Result<Success>>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
