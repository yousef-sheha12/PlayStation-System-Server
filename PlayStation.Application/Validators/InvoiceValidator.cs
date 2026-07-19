using FluentValidation;
using PlayStation.Application.DTOs.Invoice;

namespace PlayStation.Application.Validators;

public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceDto>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("Session is required");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount must be greater than or equal to 0");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Tax rate must not exceed 100%");
    }
}
