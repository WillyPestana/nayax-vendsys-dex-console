using FluentAssertions;
using NayaxVendSys.Application.Features.Dex.UploadDex;

namespace NayaxVendSys.UnitTests.Dex;

public sealed class UploadDexCommandValidatorTests
{
    private readonly UploadDexCommandValidator _validator = new();

    [Fact]
    public void Validate_AllowsTextDexSampleFiles()
    {
        using var stream = new MemoryStream("DEX"u8.ToArray());
        var command = new UploadDexCommand(stream, "sample.txt", stream.Length);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_RejectsUnsupportedExtensions()
    {
        using var stream = new MemoryStream("DEX"u8.ToArray());
        var command = new UploadDexCommand(stream, "sample.pdf", stream.Length);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UploadDexCommand.FileName));
    }

    [Fact]
    public void Validate_RejectsEmptyFiles()
    {
        using var stream = new MemoryStream();
        var command = new UploadDexCommand(stream, "sample.dex", 0);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UploadDexCommand.FileSizeBytes));
    }

    [Fact]
    public void Validate_RejectsFilesOverMaximumSize()
    {
        using var stream = new MemoryStream("DEX"u8.ToArray());
        var command = new UploadDexCommand(stream, "sample.dex", UploadDexCommandValidator.MaxFileSizeBytes + 1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UploadDexCommand.FileSizeBytes));
    }

    [Fact]
    public void Validate_RejectsUnsafeFileNames()
    {
        using var stream = new MemoryStream("DEX"u8.ToArray());
        var command = new UploadDexCommand(stream, "../sample.dex", stream.Length);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UploadDexCommand.FileName));
    }
}
