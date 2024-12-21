using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Categories.Commands;
using Admin.Application.Common.Interfaces;
using FluentValidation;

namespace Admin.Application.Categories.Validators;
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaDescription));

        RuleFor(x => x.ParentCategoryId)
            .MustAsync(CategoryExists)
            .When(x => x.ParentCategoryId.HasValue)
            .WithMessage("Parent category does not exist");
    }

    private async Task<bool> CategoryExists(Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue) return true;
        return await _categoryRepository.ExistsAsync(categoryId.Value, cancellationToken);
    }
}