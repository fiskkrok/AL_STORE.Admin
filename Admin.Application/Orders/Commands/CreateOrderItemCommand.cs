using FluentValidation;

namespace Admin.Application.Orders.Commands;
public record CreateOrderItemCommand
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
