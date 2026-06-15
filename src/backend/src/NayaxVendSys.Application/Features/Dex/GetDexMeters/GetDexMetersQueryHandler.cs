using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Application.Contracts.Dex;

namespace NayaxVendSys.Application.Features.Dex.GetDexMeters;

public sealed class GetDexMetersQueryHandler(IDexMeterRepository repository)
{
    public Task<IReadOnlyCollection<DexMeterDto>> HandleAsync(CancellationToken cancellationToken)
    {
        return repository.GetAllAsync(cancellationToken);
    }
}
