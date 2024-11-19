using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using FluentValidation;

namespace Admin.Application.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(v => v.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be in ISO 4217 format");

        RuleFor(v => v.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("Category is required")
            .MustAsync(CategoryExists).WithMessage("Category does not exist");

        RuleFor(v => v.SubCategoryId)
            .MustAsync(SubCategoryExists).WithMessage("SubCategory does not exist")
            .When(v => v.SubCategoryId.HasValue);

        RuleFor(v => v.Images)
            .NotEmpty().WithMessage("At least one image is required")
            .Must(x => x.Count <= 10).WithMessage("Maximum 10 images allowed");

        RuleForEach(v => v.Images)
            .Must(BeValidImage).WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png")
            .Must(BeValidSize).WithMessage("Image size must be less than 5MB");
    }

    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _categoryRepository.ExistsAsync(categoryId, cancellationToken);
    }

    private async Task<bool> SubCategoryExists(Guid? subCategoryId, CancellationToken cancellationToken)
    {
        if (!subCategoryId.HasValue) return true;
        return await _categoryRepository.ExistsAsync(subCategoryId.Value, cancellationToken);
    }

    private bool BeValidImage(FileUploadRequest file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    private bool BeValidSize(FileUploadRequest file)
    {
        const int maxSize = 5 * 1024 * 1024; // 5MB
        return file.Length <= maxSize;
    }
}