using FluentValidation;
using PersonPhone.Application.DTOs.Person;
using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Application.Validators.Person;

public class UpdatePersonRequestDtoValidator : AbstractValidator<UpdatePersonRequest>
{
    public UpdatePersonRequestDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(p => p.Cpf)
            .NotEmpty().WithMessage("Cpf is required.")
            .Must(Cpf.IsValid).WithMessage("Cpf is invalid.");

        RuleFor(p => p.BirthDate)
            .NotNull().WithMessage("BirthDate is required.")
            .LessThan(DateTime.Today).WithMessage("BirthDate must be in the past.");
    }
}
