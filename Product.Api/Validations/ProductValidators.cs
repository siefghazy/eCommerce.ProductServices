using FluentValidation;
using Product.Core.Dtos;
using System.Data;

namespace eCommerce.ProductServices.Validations
{
    public class ProductValidators: AbstractValidator<ProductDto>
    {
        public ProductValidators()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required.")
                .MaximumLength(500).WithMessage("Product description cannot exceed 500 characters.");
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Product price must be greater than zero.");
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Product quantity cannot be negative.");
            

        }
    }
}
