using FluentValidation;

namespace ECommerceBackend.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(72);
        }
    }
}