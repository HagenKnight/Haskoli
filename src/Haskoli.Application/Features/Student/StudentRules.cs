using FluentValidation;

namespace Haskoli.Application.Features.Student
{
    /// <summary>
    /// Reglas compartidas por los validadores de creación y actualización. Viven aquí para que
    /// ambos no puedan divergir: la especificación exige las mismas reglas en los dos casos.
    /// </summary>
    internal static class StudentRules
    {
        public const int DocumentMaxLength = 20;
        public const int NameMaxLength = 100;
        public const int EmailMaxLength = 150;

        /* FluentValidation acepta 'usuario@dominio' con su validador de email, pero la
           especificación exige un dominio con punto y sufijo, así que el patrón lo impone:
           parte local sin arroba ni espacios, arroba, dominio y al menos un punto seguido
           de sufijo. */
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s.]+$";

        public static IRuleBuilderOptions<T, string> DocumentRules<T>(this IRuleBuilderInitial<T, string> rule) =>
            rule.Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El documento es obligatorio.")
                .MaximumLength(DocumentMaxLength).WithMessage($"El documento no puede superar los {DocumentMaxLength} caracteres.");

        public static IRuleBuilderOptions<T, string> FirstNameRules<T>(this IRuleBuilderInitial<T, string> rule) =>
            rule.Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(NameMaxLength).WithMessage($"El nombre no puede superar los {NameMaxLength} caracteres.");

        public static IRuleBuilderOptions<T, string> LastNameRules<T>(this IRuleBuilderInitial<T, string> rule) =>
            rule.Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(NameMaxLength).WithMessage($"El apellido no puede superar los {NameMaxLength} caracteres.");

        public static IRuleBuilderOptions<T, string> EmailRules<T>(this IRuleBuilderInitial<T, string> rule) =>
            rule.Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .MaximumLength(EmailMaxLength).WithMessage($"El email no puede superar los {EmailMaxLength} caracteres.")
                .Matches(EmailPattern).WithMessage("El email no tiene un formato válido.");

        public static IRuleBuilderOptions<T, int> IdRules<T>(this IRuleBuilderInitial<T, int> rule) =>
            rule.GreaterThan(0).WithMessage("El Id debe ser mayor que cero.");
    }
}
