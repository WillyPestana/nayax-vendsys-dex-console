using System.Text;
using FluentValidation;
using NayaxVendSys.Application.Abstractions.Parsing;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Application.Abstractions.Realtime;
using NayaxVendSys.Application.Contracts.Dex;
using NayaxVendSys.Domain.Exceptions;

namespace NayaxVendSys.Application.Features.Dex.UploadDex;

public sealed class UploadDexCommandHandler(
    IValidator<UploadDexCommand> validator,
    IDexFileParser parser,
    IDexMeterRepository repository,
    IDexProcessingNotifier notifier)
{
    public async Task<UploadDexResponse> HandleAsync(
        UploadDexCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await notifier.NotifyAsync("UploadReceived", "DEX file received by the API.", cancellationToken);

        using var reader = new StreamReader(command.FileStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        EnsureTextFile(content);

        await notifier.NotifyAsync("ParsingStarted", "Parsing DEX segments.", cancellationToken);
        var parsedDocument = parser.Parse(content);
        await notifier.NotifyAsync("ParsingCompleted", $"Parsed {parsedDocument.Lanes.Count} lane meters.", cancellationToken);

        await notifier.NotifyAsync("PersistenceStarted", "Saving DEX meter and lanes.", cancellationToken);
        var dexMeterId = await repository.SaveAsync(parsedDocument, cancellationToken);
        await notifier.NotifyAsync("PersistenceCompleted", "DEX data saved.", cancellationToken);

        return new UploadDexResponse(
            dexMeterId,
            parsedDocument.MachineId,
            parsedDocument.DexDateTime,
            parsedDocument.Lanes.Count,
            "DEX file processed successfully.");
    }

    private static void EnsureTextFile(string content)
    {
        if (content.Contains('\0'))
        {
            throw new DexParsingException("The uploaded file appears to contain binary data.");
        }
    }
}
