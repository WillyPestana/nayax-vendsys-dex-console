using System.Data;
using Dapper;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Application.Contracts.Dex;
using NayaxVendSys.Application.Features.Dex.Parsing;
using NayaxVendSys.Infrastructure.Persistence.Connection;
using NayaxVendSys.Infrastructure.Persistence.StoredProcedures;

namespace NayaxVendSys.Infrastructure.Persistence;

public sealed class DexMeterRepository(ISqlConnectionFactory connectionFactory) : IDexMeterRepository
{
    public async Task<int> SaveAsync(ParsedDexDocument document, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateApplicationConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var meterParameters = new DynamicParameters();
            meterParameters.Add("@MachineId", document.MachineId, DbType.String, size: 20);
            meterParameters.Add("@DEXDateTime", document.DexDateTime, DbType.DateTime2);
            meterParameters.Add("@MachineSerialNumber", document.MachineSerialNumber, DbType.String, size: 20);
            meterParameters.Add("@ValueOfPaidVends", document.ValueOfPaidVends, DbType.Decimal);
            meterParameters.Add("@DEXMeterId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(new CommandDefinition(
                StoredProcedureNames.SaveDexMeter,
                meterParameters,
                transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var dexMeterId = meterParameters.Get<int>("@DEXMeterId");

            foreach (var lane in document.Lanes)
            {
                var laneParameters = new DynamicParameters();
                laneParameters.Add("@DEXMeterId", dexMeterId, DbType.Int32);
                laneParameters.Add("@ProductIdentifier", lane.ProductIdentifier, DbType.String, size: 6);
                laneParameters.Add("@Price", lane.Price, DbType.Decimal);
                laneParameters.Add("@NumberOfVends", lane.NumberOfVends, DbType.Int32);
                laneParameters.Add("@ValueOfPaidSales", lane.ValueOfPaidSales, DbType.Decimal);

                await connection.ExecuteAsync(new CommandDefinition(
                    StoredProcedureNames.SaveDexLaneMeter,
                    laneParameters,
                    transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return dexMeterId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<DexMeterDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                m.Id,
                m.MachineId,
                m.DEXDateTime,
                m.MachineSerialNumber,
                m.ValueOfPaidVends,
                l.Id AS LaneId,
                l.ProductIdentifier,
                l.Price,
                l.NumberOfVends,
                l.ValueOfPaidSales
            FROM dbo.DEXMeter AS m
            LEFT JOIN dbo.DEXLaneMeter AS l ON l.DEXMeterId = m.Id
            ORDER BY m.DEXDateTime DESC, m.Id DESC, l.ProductIdentifier;
            """;

        await using var connection = connectionFactory.CreateApplicationConnection();
        var rows = await connection.QueryAsync<DexMeterFlatRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));

        var meters = new Dictionary<int, DexMeterAccumulator>();
        foreach (var row in rows)
        {
            if (!meters.TryGetValue(row.Id, out var meter))
            {
                meter = new DexMeterAccumulator(
                    row.Id,
                    row.MachineId,
                    row.DEXDateTime,
                    row.MachineSerialNumber,
                    row.ValueOfPaidVends);
                meters.Add(row.Id, meter);
            }

            if (row.LaneId.HasValue)
            {
                meter.Lanes.Add(new DexLaneMeterDto(
                    row.LaneId.Value,
                    row.ProductIdentifier ?? string.Empty,
                    row.Price ?? 0m,
                    row.NumberOfVends ?? 0,
                    row.ValueOfPaidSales ?? 0m));
            }
        }

        return meters.Values
            .Select(meter => new DexMeterDto(
                meter.Id,
                meter.MachineId,
                meter.DexDateTime,
                meter.MachineSerialNumber,
                meter.ValueOfPaidVends,
                meter.Lanes))
            .ToArray();
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateApplicationConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            StoredProcedureNames.ClearDexData,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    private sealed class DexMeterFlatRow
    {
        public int Id { get; init; }

        public string MachineId { get; init; } = string.Empty;

        public DateTime DEXDateTime { get; init; }

        public string MachineSerialNumber { get; init; } = string.Empty;

        public decimal ValueOfPaidVends { get; init; }

        public int? LaneId { get; init; }

        public string? ProductIdentifier { get; init; }

        public decimal? Price { get; init; }

        public int? NumberOfVends { get; init; }

        public decimal? ValueOfPaidSales { get; init; }
    }

    private sealed record DexMeterAccumulator(
        int Id,
        string MachineId,
        DateTime DexDateTime,
        string MachineSerialNumber,
        decimal ValueOfPaidVends)
    {
        public List<DexLaneMeterDto> Lanes { get; } = [];
    }
}
