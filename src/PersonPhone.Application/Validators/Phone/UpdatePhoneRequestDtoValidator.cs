using FluentValidation;
using PersonPhone.Application.DTOs.Phone;

namespace PersonPhone.Application.Validators.Phone;

public class UpdatePhoneRequestDtoValidator : AbstractValidator<UpdatePhoneRequest>
{
    public UpdatePhoneRequestDtoValidator()
    {
        RuleFor(p => p.Type)
            .IsInEnum().WithMessage("Type is invalid.");

        RuleFor(p => p.Number)
            .NotEmpty().WithMessage("Number is required.")
            .Length(10, 11).WithMessage("Number must be between 10 and 11 characters.");
    }
}
