using FluentValidation;
using PlayStation.Application.DTOs.Device;

namespace PlayStation.Application.Validators;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceDto>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Device name is required")
            .MaximumLength(100).WithMessage("Device name must not exceed 100 characters");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Hourly rate must be greater than 0");
    }
}

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceDto>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Device name is required")
            .MaximumLength(100).WithMessage("Device name must not exceed 100 characters");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Hourly rate must be greater than 0");
    }
}
