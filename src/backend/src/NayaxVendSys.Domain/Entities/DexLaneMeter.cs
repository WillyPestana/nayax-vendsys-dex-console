namespace NayaxVendSys.Domain.Entities;

public sealed class DexLaneMeter
{
    public DexLaneMeter(
        int id,
        string productIdentifier,
        decimal price,
        int numberOfVends,
        decimal valueOfPaidSales)
    {
        Id = id;
        ProductIdentifier = productIdentifier;
        Price = price;
        NumberOfVends = numberOfVends;
        ValueOfPaidSales = valueOfPaidSales;
    }

    public int Id { get; }

    public string ProductIdentifier { get; }

    public decimal Price { get; }

    public int NumberOfVends { get; }

    public decimal ValueOfPaidSales { get; }
}
