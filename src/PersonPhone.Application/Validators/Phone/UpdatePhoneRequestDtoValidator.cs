using FluentValidation;
using PersonPhone.Application.DTOs.Phone;
using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Application.Validators.Phone;

public class UpdatePhoneRequestDtoValidator : AbstractValidator<UpdatePhoneRequest>
{
    public UpdatePhoneRequestDtoValidator()
    {
        RuleFor(p => p.Type)
            .IsInEnum().WithMessage("Type is invalid.");

        RuleFor(p => p.Number)
            .NotEmpty().WithMessage("Number is required.")
            .Must(PhoneNumber.IsValid).WithMessage("Number is invalid.");
    }
}
