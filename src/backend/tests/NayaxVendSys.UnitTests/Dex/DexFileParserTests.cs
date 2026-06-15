using FluentAssertions;
using NayaxVendSys.Domain.Exceptions;
using NayaxVendSys.Infrastructure.Parsing;

namespace NayaxVendSys.UnitTests.Dex;

public sealed class DexFileParserTests
{
    private readonly DexFileParser _parser = new();

    [Fact]
    public void Parse_ExtractsRequiredMeterAndLaneFields_FromMachineA()
    {
        var content = ReadSample("dex-machine-a.txt");

        var result = _parser.Parse(content);

        result.MachineId.Should().Be("100077238");
        result.MachineSerialNumber.Should().Be("100077238");
        result.DexDateTime.Should().Be(new DateTime(2023, 12, 10, 23, 10, 53));
        result.ValueOfPaidVends.Should().Be(344.50m);
        result.Lanes.Should().HaveCount(38);

        result.Lanes.First().Should().BeEquivalentTo(new
        {
            ProductIdentifier = "101",
            Price = 3.25m,
            NumberOfVends = 4,
            ValueOfPaidSales = 13.00m
        });
    }

    [Fact]
    public void Parse_ExtractsRequiredMeterAndLaneFields_FromMachineB()
    {
        var content = ReadSample("dex-machine-b.txt");

        var result = _parser.Parse(content);

        result.MachineId.Should().Be("302029479");
        result.MachineSerialNumber.Should().Be("302029479");
        result.DexDateTime.Should().Be(new DateTime(2023, 12, 10, 23, 11, 45));
        result.ValueOfPaidVends.Should().Be(4758.85m);
        result.Lanes.Should().HaveCount(35);

        result.Lanes.First().Should().BeEquivalentTo(new
        {
            ProductIdentifier = "101",
            Price = 3.25m,
            NumberOfVends = 75,
            ValueOfPaidSales = 239.25m
        });
    }

    [Fact]
    public void Parse_ThrowsClearError_WhenRequiredMeterFieldIsMissing()
    {
        const string content = """
            DXS*STF0000000*VA*V0/6*1
            ID5*20231210*2310*53*
            VA1*100*1
            PA1*101*325
            PA2*1*325
            """;

        var act = () => _parser.Parse(content);

        act.Should()
            .Throw<DexParsingException>()
            .WithMessage("*ID101/MachineId*");
    }

    [Fact]
    public void Parse_ThrowsClearError_WhenFileDoesNotStartWithDexHeader()
    {
        const string content = """
            ID1*100077238
            ID5*20231210*2310*53*
            VA1*100*1
            PA1*101*325
            PA2*1*325
            """;

        var act = () => _parser.Parse(content);

        act.Should()
            .Throw<DexParsingException>()
            .WithMessage("*first segment must be DXS*");
    }

    private static string ReadSample(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "samples", fileName);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find sample DEX file {fileName}.");
    }
}
