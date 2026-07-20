using FluentValidation;
using PlayStation.Application.DTOs.Session;

namespace PlayStation.Application.Validators;

public class StartSessionValidator : AbstractValidator<StartSessionDto>
{
    public StartSessionValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0).WithMessage("Device ID is required");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Hourly rate must be greater than 0");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");
    }
}

public class AddProductToSessionValidator : AbstractValidator<AddProductToSessionDto>
{
    public AddProductToSessionValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}

public class EndSessionValidator : AbstractValidator<EndSessionDto>
{
    public EndSessionValidator()
    {
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");
    }
}
