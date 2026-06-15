namespace NayaxVendSys.Domain.Entities;

public sealed class DexMeter
{
    private readonly List<DexLaneMeter> _lanes = [];

    public DexMeter(
        int id,
        string machineId,
        DateTime dexDateTime,
        string machineSerialNumber,
        decimal valueOfPaidVends,
        IEnumerable<DexLaneMeter>? lanes = null)
    {
        Id = id;
        MachineId = machineId;
        DexDateTime = dexDateTime;
        MachineSerialNumber = machineSerialNumber;
        ValueOfPaidVends = valueOfPaidVends;

        if (lanes is not null)
        {
            _lanes.AddRange(lanes);
        }
    }

    public int Id { get; }

    public string MachineId { get; }

    public DateTime DexDateTime { get; }

    public string MachineSerialNumber { get; }

    public decimal ValueOfPaidVends { get; }

    public IReadOnlyCollection<DexLaneMeter> Lanes => _lanes;
}
