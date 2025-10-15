using FluentValidation;
using ModuleHeatMap.Application.DTOs;

namespace ModuleHeatMap.Application.Validators;

public class ModuleAccessValidator : AbstractValidator<ModuleAccessDto>
{
    public ModuleAccessValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("ApplicationId é obrigatório");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId é obrigatório");

        RuleFor(x => x.ModuleName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("ModuleName é obrigatório e deve ter no máximo 100 caracteres");

        RuleFor(x => x.ModuleUrl)
            .NotEmpty()
            .WithMessage("ModuleUrl é obrigatório");

        RuleFor(x => x.AccessType)
            .IsInEnum()
            .WithMessage("AccessType deve ser um valor válido");
    }
}
