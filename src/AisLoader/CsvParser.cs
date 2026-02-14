using System;
using System.Globalization;

namespace AisLoader;

public static class CsvParser
{
    public static AisRecord ParseAisRecord(string line)
    {
        var fields = line.Split(',');
        return new AisRecord(
            Timestamp: DateTime.ParseExact(fields[0], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            TypeOfMobile: TrimAndNullify(fields[1]),
            Mmsi: int.Parse(fields[2]),
            Latitude: ParseDouble(fields[3]),
            Longitude: ParseDouble(fields[4]),
            NavigationalStatus: TrimAndNullify(fields[5]),
            Rot: ParseDouble(fields[6]),
            Sog: ParseDouble(fields[7]),
            Cog: ParseDouble(fields[8]),
            Heading: ParseInt(fields[9]),
            Imo: ParseNullableInt(fields[10]),
            Callsign: TrimAndNullify(fields[11]),
            Name: TrimAndNullify(fields[12]),
            ShipType: TrimAndNullify(fields[13]),
            CargoType: TrimAndNullify(fields[14]),
            Width: ParseNullableInt(fields[15]),
            Length: ParseNullableInt(fields[16]),
            TypeOfPositionFixingDevice: TrimAndNullify(fields[17]),
            Draught: ParseNullableDouble(fields[18]),
            Destination: TrimAndNullify(fields[19]),
            Eta: TrimAndNullify(fields[20]),
            DataSourceType: TrimAndNullify(fields[21]),
            DimensionA: ParseNullableInt(fields[22]),
            DimensionB: ParseNullableInt(fields[23]),
            DimensionC: ParseNullableInt(fields[24]),
            DimensionD: ParseNullableInt(fields[25]));
    }

    private static string? TrimAndNullify(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static double ParseDouble(string s) =>
        double.TryParse(s, out var result) ? result : 0;

    private static int ParseInt(string s) =>
        string.IsNullOrWhiteSpace(s) ? -1 : int.Parse(s);

    private static int? ParseNullableInt(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : (int.TryParse(s, out var result) ? result : null);

    private static double? ParseNullableDouble(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : (double.TryParse(s, out var result) ? result : null);
}
