using System.Globalization;
using NayaxVendSys.Application.Abstractions.Parsing;
using NayaxVendSys.Application.Features.Dex.Parsing;
using NayaxVendSys.Domain.Exceptions;

namespace NayaxVendSys.Infrastructure.Parsing;

public sealed class DexFileParser : IDexFileParser
{
    private const int DefaultCurrencyDecimalPlaces = 2;

    public ParsedDexDocument Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DexParsingException("The DEX file is empty.");
        }

        string? machineId = null;
        string? machineSerialNumber = null;
        DateTime? dexDateTime = null;
        decimal? valueOfPaidVends = null;
        var currencyDecimalPlaces = DefaultCurrencyDecimalPlaces;
        var lanes = new List<ParsedDexLane>();
        PendingLane? pendingLane = null;

        var lines = content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        EnsureDexHeader(lines);

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var parts = lines[index].Split('*');
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0])
            {
                case "ID1":
                    machineSerialNumber = Required(parts, 1, "ID101", lineNumber);
                    // The challenge names ID106 for MachineId but its expected sample values are ID101.
                    machineId = machineSerialNumber;
                    break;

                case "ID4":
                    currencyDecimalPlaces = ParseInteger(Required(parts, 1, "ID401", lineNumber), "ID401", lineNumber);
                    break;

                case "ID5":
                    dexDateTime = ParseDexDateTime(parts, lineNumber);
                    break;

                case "VA1":
                    valueOfPaidVends = ParseCurrency(
                        Required(parts, 1, "VA101", lineNumber),
                        currencyDecimalPlaces,
                        "VA101",
                        lineNumber);
                    break;

                case "PA1":
                    if (pendingLane is not null)
                    {
                        throw new DexParsingException($"PA1 at line {lineNumber} started before PA2 completed the previous lane.");
                    }

                    pendingLane = new PendingLane(
                        Required(parts, 1, "PA101", lineNumber),
                        ParseCurrency(Required(parts, 2, "PA102", lineNumber), currencyDecimalPlaces, "PA102", lineNumber),
                        lineNumber);
                    break;

                case "PA2":
                    if (pendingLane is null)
                    {
                        throw new DexParsingException($"PA2 at line {lineNumber} does not have a preceding PA1 lane segment.");
                    }

                    lanes.Add(new ParsedDexLane(
                        pendingLane.ProductIdentifier,
                        pendingLane.Price,
                        ParseInteger(Required(parts, 1, "PA201", lineNumber), "PA201", lineNumber),
                        ParseCurrency(Required(parts, 2, "PA202", lineNumber), currencyDecimalPlaces, "PA202", lineNumber)));
                    pendingLane = null;
                    break;
            }
        }

        if (pendingLane is not null)
        {
            throw new DexParsingException($"PA1 at line {pendingLane.LineNumber} was not followed by PA2.");
        }

        return new ParsedDexDocument(
            RequiredParsed(machineId, "ID101/MachineId"),
            RequiredParsed(dexDateTime, "ID5/DEXDateTime"),
            RequiredParsed(machineSerialNumber, "ID101/MachineSerialNumber"),
            RequiredParsed(valueOfPaidVends, "VA101/ValueOfPaidVends"),
            lanes.Count > 0 ? lanes : throw new DexParsingException("The DEX file does not contain any PA1/PA2 lane meters."));
    }

    private static DateTime ParseDexDateTime(string[] parts, int lineNumber)
    {
        var date = Required(parts, 1, "ID501", lineNumber);
        var time = Required(parts, 2, "ID502", lineNumber);
        var seconds = Optional(parts, 3);

        var parsedDate = ParseDate(date, "ID501", lineNumber);
        var parsedTime = ParseTime(time, seconds, lineNumber);

        return parsedDate.Date.Add(parsedTime);
    }

    private static void EnsureDexHeader(string[] lines)
    {
        if (lines.Length == 0 || !lines[0].StartsWith("DXS*", StringComparison.OrdinalIgnoreCase))
        {
            throw new DexParsingException("The uploaded file is not a valid DEX audit file. The first segment must be DXS.");
        }
    }

    private static DateTime ParseDate(string value, string elementName, int lineNumber)
    {
        var format = value.Length switch
        {
            6 => "yyMMdd",
            8 => "yyyyMMdd",
            _ => throw new DexParsingException($"{elementName} at line {lineNumber} must be YYMMDD or YYYYMMDD.")
        };

        if (DateTime.TryParseExact(
            value,
            format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed))
        {
            return parsed;
        }

        throw new DexParsingException($"{elementName} at line {lineNumber} is not a valid DEX date.");
    }

    private static TimeSpan ParseTime(string time, string? seconds, int lineNumber)
    {
        if (time.Length is not (4 or 6))
        {
            throw new DexParsingException($"ID502 at line {lineNumber} must be HHMM or HHMMSS.");
        }

        var hour = ParseInteger(time[..2], "ID502 hour", lineNumber);
        var minute = ParseInteger(time[2..4], "ID502 minute", lineNumber);
        var second = time.Length == 6
            ? ParseInteger(time[4..6], "ID502 second", lineNumber)
            : ParseInteger(string.IsNullOrWhiteSpace(seconds) ? "0" : seconds, "ID503", lineNumber);

        try
        {
            return new TimeSpan(hour, minute, second);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new DexParsingException($"ID5 at line {lineNumber} contains an invalid time.", exception);
        }
    }

    private static decimal ParseCurrency(string value, int decimalPlaces, string elementName, int lineNumber)
    {
        var integer = ParseInteger(value, elementName, lineNumber);
        return integer / Pow10(decimalPlaces);
    }

    private static int ParseInteger(string value, string elementName, int lineNumber)
    {
        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        throw new DexParsingException($"{elementName} at line {lineNumber} must be a whole number.");
    }

    private static decimal Pow10(int decimalPlaces)
    {
        if (decimalPlaces is < 0 or > 6)
        {
            throw new DexParsingException("ID401 currency decimal places must be between 0 and 6.");
        }

        var result = 1m;
        for (var index = 0; index < decimalPlaces; index++)
        {
            result *= 10m;
        }

        return result;
    }

    private static string Required(string[] parts, int index, string elementName, int lineNumber)
    {
        var value = Optional(parts, index);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        throw new DexParsingException($"{elementName} is missing at line {lineNumber}.");
    }

    private static string? Optional(string[] parts, int index)
    {
        return index < parts.Length ? parts[index] : null;
    }

    private static T RequiredParsed<T>(T? value, string elementName)
        where T : struct
    {
        return value ?? throw new DexParsingException($"{elementName} is missing.");
    }

    private static string RequiredParsed(string? value, string elementName)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new DexParsingException($"{elementName} is missing.");
    }

    private sealed record PendingLane(string ProductIdentifier, decimal Price, int LineNumber);
}
