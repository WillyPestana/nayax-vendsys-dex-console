using FluentValidation;

namespace NayaxVendSys.Application.Features.Dex.UploadDex;

public sealed class UploadDexCommandValidator : AbstractValidator<UploadDexCommand>
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const int MaxFileNameLength = 120;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dex",
        ".txt",
        ".dts"
    };

    public UploadDexCommandValidator()
    {
        RuleFor(command => command.FileStream)
            .NotNull()
            .Must(stream => stream.CanRead)
            .WithMessage("The uploaded DEX file cannot be read.");

        RuleFor(command => command.FileName)
            .NotEmpty()
            .MaximumLength(MaxFileNameLength)
            .WithMessage($"The uploaded file name must be {MaxFileNameLength} characters or fewer.")
            .Must(HaveSafeFileName)
            .WithMessage("The uploaded file name is invalid.")
            .Must(HaveAllowedExtension)
            .WithMessage("The uploaded file must use .dex, .dts or .txt extension.");

        RuleFor(command => command.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"DEX file size must be between 1 byte and {MaxFileSizeBytes} bytes.");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        return AllowedExtensions.Contains(Path.GetExtension(fileName));
    }

    private static bool HaveSafeFileName(string fileName)
    {
        return fileName.IndexOfAny(['/', '\\']) < 0
            && fileName.All(character => !char.IsControl(character));
    }
}
