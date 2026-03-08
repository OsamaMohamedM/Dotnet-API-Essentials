using FluentValidation;
using Api_Pipeline_Concepts.DTOs;

namespace Api_Pipeline_Concepts.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Category)
            .NotEmpty();
    }
}
