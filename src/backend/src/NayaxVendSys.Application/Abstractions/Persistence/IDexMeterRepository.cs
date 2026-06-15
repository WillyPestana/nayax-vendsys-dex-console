using NayaxVendSys.Application.Contracts.Dex;
using NayaxVendSys.Application.Features.Dex.Parsing;

namespace NayaxVendSys.Application.Abstractions.Persistence;

public interface IDexMeterRepository
{
    Task<int> SaveAsync(ParsedDexDocument document, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DexMeterDto>> GetAllAsync(CancellationToken cancellationToken);

    Task ClearAsync(CancellationToken cancellationToken);
}
